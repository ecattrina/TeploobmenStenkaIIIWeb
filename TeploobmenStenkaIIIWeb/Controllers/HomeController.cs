using Microsoft.AspNetCore.Mvc;
using TeploobmenStenkaIIIWeb.Services;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using TeploobmenStenkaIIIWeb.Models.InputModels.Direct;
using HeatTransferCalculator.Models;
using TeploobmenStenkaIIIWeb.Models.InputModels.Inverse;
using TeploobmenStenkaIIIWeb.Models.Entities;

public class HeatController : Controller
{
    private readonly ExcelBiotCoefficientService _dbAccessService;

    public HeatController(ExcelBiotCoefficientService excelService)
    {
        _dbAccessService = excelService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DirectProblem()
    {
        return View(new DirectCalculationModel());
    }
    
    [HttpPost]
    public async Task<IActionResult> DirectProblem(DirectCalculationModel model)
    {
        var endTimeSeconds = model.Time;
        var minTimeInterval = model.MinTimeInterval;
        var desiredTimeInterval = model.TimeInterval;

        // ��������� � ����� �� ������� � ������� ���������
        if (!ModelState.IsValid 
            || desiredTimeInterval < minTimeInterval
            || desiredTimeInterval > endTimeSeconds)
        {
            model.WarningMessage = $"Неверно выбран временной интервал среза: {desiredTimeInterval} секунд. Допустимый интервал: {Math.Round(minTimeInterval, 0)} - {endTimeSeconds} секунд.";
            return View(model);
        }
        // 1. ������ ������������ ���������������������� (��� � Excel)
        double a = Math.Round(
            model.ThermalConductivity / (model.Density * model.HeatCapacity),
            6); // 6 ������ ����� �������

        // 2. ������ ����� ��� (��� � Excel)
        model.BiotNumber = Math.Round(
            (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
            2); // 2 ����� ��� � Excel

        // 3. ��������� ������������� (������ ������������ Excel)
        var coefficients = await _dbAccessService.GetCoefficients(model.BiotNumber);

        var timeEllapsed = 0.0;
        do
        {
            var diagramData = CountTempsWithinTime(model, a, coefficients, timeEllapsed);

            model.DiagramData.Add(diagramData);

        } while ((timeEllapsed += desiredTimeInterval) <= endTimeSeconds);

        var dataInFinalPoint = CountTempsWithinTime(model, a, coefficients, model.Time);

        if (!model.DiagramData.Any(x => x.TimeStamp == model.Time))
            model.DiagramData.Add(dataInFinalPoint);

        model.FourierNumber = dataInFinalPoint.FourierNumber;
        model.AverageTemperature = dataInFinalPoint.AverageTemp;
        model.SurfaceTemperature = dataInFinalPoint.SurfaceTemp;
        model.CenterTemperature = dataInFinalPoint.CenterTemp;  

        if (model.FourierNumber <= 0.3
                || double.IsNaN(model.CenterTemperature)
                || double.IsNaN(model.AverageTemperature)
                || double.IsNaN(model.SurfaceTemperature))
        {
            ModelState.AddModelError("", "������: �������� ������������ ���������� �������. ��������� ��������� ���������.");
            return View(model);
        }

        return View("DirectResult", model);
    }

    private DirectDiagramData CountTempsWithinTime(
        DirectCalculationModel model, double a, 
        BioCoeff coefficients, double timeEllapsed)
    {
        // Если t=0, все температуры равны начальной
        if (timeEllapsed == 0)
        {
            return new DirectDiagramData
            {
                FourierNumber = 0,
                CenterTemp = model.InitialTemperature,
                AverageTemp = model.InitialTemperature,
                SurfaceTemp = model.InitialTemperature,
                TimeStamp = 0
            };
        }
        // 4. Число Фурье (без округления)
        var fourierNum = (a * timeEllapsed) / Math.Pow(model.Thickness / 2, 2);

        // 5. Безразмерные температуры (без округления)
        double thetaCenter = coefficients.Np * Math.Exp(-coefficients.MuSquared * fourierNum);
        double thetaAverage = coefficients.M * Math.Exp(-coefficients.MuSquared * fourierNum);
        double thetaSurface = coefficients.Pp * Math.Exp(-coefficients.MuSquared * fourierNum);

        // 6. Температуры (округлять только для вывода)
        var centerTemperature = model.EnvironmentTemperature +
            (model.InitialTemperature - model.EnvironmentTemperature) * thetaCenter;
        var averageTemperature = model.EnvironmentTemperature +
            (model.InitialTemperature - model.EnvironmentTemperature) * thetaAverage;
        var surfaceTemperature = model.EnvironmentTemperature +
            (model.InitialTemperature - model.EnvironmentTemperature) * thetaSurface;

        var diagramData = new DirectDiagramData
        {
            FourierNumber = fourierNum,
            CenterTemp = centerTemperature,
            AverageTemp = averageTemperature,
            SurfaceTemp = surfaceTemperature,
            TimeStamp = timeEllapsed
        };

        return diagramData;
    }

    [HttpGet]
    public IActionResult InverseProblem()
    {
        return View(new InverseCalculationModel());
    }

    [HttpPost] 
    public async Task<IActionResult> InverseProblem(InverseCalculationModel model)
    {
        // Проверка на валидность
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Расчет коэффициента температуропроводности
        double a = model.ThermalConductivity / (model.Density * model.HeatCapacity);

        // 2. Расчет числа Био
        model.BiotNumber = Math.Round(
            (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
            2);

        // 3. Получение коэффициентов
        var coefficients = await _dbAccessService.GetCoefficients(model.BiotNumber);

        // Новый расчет: шаги по температуре с учетом направления
        int direction = Math.Sign(model.TargetTemperature - model.InitialTemperature);
        double tempDiff = Math.Abs(model.TargetTemperature - model.InitialTemperature);
        int pointsCount = (int)model.TempPoints;
        double stepSize = tempDiff / (pointsCount - 1);

        model.DiagramData.Clear();
        for (int i = 0; i < pointsCount; i++)
        {
            double currentTemp = model.InitialTemperature + direction * i * stepSize;
            if (i == 0)
            {
                model.DiagramData.Add(new InverseDiagramData
                {
                    TimeStamp = 0,
                    CenterTemp = currentTemp,
                    FourierNumber = 0
                });
            }
            else
            {
                var diagramData = CountWithinTemp(model, a, coefficients, currentTemp);
                model.DiagramData.Add(diagramData);
            }
        }
        model.FourierNumber = model.DiagramData.Last().FourierNumber;
        model.Time = model.DiagramData.Last().TimeStamp;

        // Проверка на корректность результата
        if (double.IsNaN(model.Time) || model.Time <= 0)
        {
            model.WarningMessage = "Получено некорректное значение времени. Проверьте введенные параметры.";
            return View(model);
        }

        if (model.FourierNumber <= 0.3)
        {
            return View(model);
        }

        return View("InverseResult", model);
    }

    private InverseDiagramData CountWithinTemp(InverseCalculationModel model, double a, BioCoeff coefficients, double temp)
    {
        // 4. ������ ������������ �����������
        double thetaCenter = (model.EnvironmentTemperature - temp) /
                           (model.EnvironmentTemperature - model.InitialTemperature);

        var log = Math.Log(thetaCenter / coefficients.Np);

        // 5. ������ ����� �����
        var fourier = Math.Round(
            (1 / -coefficients.MuSquared) * Math.Log(thetaCenter / coefficients.Np),
            2);

        // 6. ������ �������
        var time = (fourier * Math.Pow(model.Thickness / 2, 2)) / a;

        return new InverseDiagramData()
        {
            TimeStamp = time,
            CenterTemp = temp,
            FourierNumber = fourier,
        };
    }

    public async Task<IActionResult> DirectResult(DirectCalculationModel model)
    {
        return View(model);
    }
}