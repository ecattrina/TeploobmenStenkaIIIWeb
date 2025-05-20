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

        // зациклить с шагом по времени и сделать диаграммы
        if (!ModelState.IsValid 
            || desiredTimeInterval < minTimeInterval
            || desiredTimeInterval > endTimeSeconds)
        {
            return View(model);
        }
        // 1. Расчет коэффициента температуропроводности (как в Excel)
        double a = Math.Round(
            model.ThermalConductivity / (model.Density * model.HeatCapacity),
            6); // 6 знаков после запятой

        // 2. Расчет числа Био (как в Excel)
        model.BiotNumber = Math.Round(
            (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
            2); // 2 знака как в Excel

        // 3. Получение коэффициентов (точное соответствие Excel)
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
            ModelState.AddModelError("", "Ошибка: получены некорректные результаты расчета. Проверьте введенные параметры.");
            return View(model);
        }

        return View("DirectResult", model);
    }

    private DirectDiagramData CountTempsWithinTime(
        DirectCalculationModel model, double a, 
        BioCoeff coefficients, double timeEllapsed)
    {
        // 4. Расчет числа Фурье (как в Excel)
        var fourierNum = Math.Round(
            (a * timeEllapsed) / Math.Pow(model.Thickness / 2, 2),
            2);

        // 5. Расчет безразмерных температур (формулы из Excel)
        double thetaCenter = Math.Round(
            coefficients.Np * Math.Exp(-coefficients.MuSquared * fourierNum),
            4);

        double thetaAverage = Math.Round(
            coefficients.M * Math.Exp(-coefficients.MuSquared * fourierNum),
            4);

        double thetaSurface = Math.Round(
            coefficients.Pp * Math.Exp(-coefficients.MuSquared * fourierNum),
            4);

        // 6. Расчет температур (формулы и округление как в Excel)
        var centerTemperature = Math.Round(
            model.EnvironmentTemperature +
            (model.InitialTemperature - model.EnvironmentTemperature) * thetaCenter,
            0);
        var averageTemperature = Math.Round(
            model.EnvironmentTemperature +
            (model.InitialTemperature - model.EnvironmentTemperature) * thetaAverage,
            0);
        var surfaceTemperature = Math.Round(
            model.EnvironmentTemperature +
            (model.InitialTemperature - model.EnvironmentTemperature) * thetaSurface,
            0);

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
        // зациклить с шагом по темпе
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

        var tempStep = (model.InitialTemperature - model.TargetTemperature) / model.TempPoints;
        var iteration = 0;
        do
        {
            var temp = model.InitialTemperature - (iteration * tempStep);
            var diagramData = CountWithinTemp(model, a, coefficients, temp);

            model.DiagramData.Add(diagramData);

        } while ((++iteration) <= model.TempPoints);

        var dataInFinalPoint = CountWithinTemp(model, a, coefficients, model.TargetTemperature);

        model.FourierNumber = dataInFinalPoint.FourierNumber;
        model.Time = dataInFinalPoint.TimeStamp;

        // Общая проверка на корректность времени
        if (double.IsNaN(model.Time) || model.Time <= 0)
        {
            throw new Exception("Получено некорректное значение времени. Проверьте введенные параметры.");
        }

        if (model.FourierNumber <= 0.3)
        {
            // ModelState.AddModelError("", "Ошибка: получены некорректные результаты расчета. Проверьте введенные параметры.");
            return View(model);
        }

        return View("InverseResult", model);

    }

    private InverseDiagramData CountWithinTemp(InverseCalculationModel model, double a, BioCoeff coefficients, double temp)
    {
        // 4. Расчет безразмерной температуры
        double thetaCenter = (model.EnvironmentTemperature - temp) /
                           (model.EnvironmentTemperature - model.InitialTemperature);

        var log = Math.Log(thetaCenter / coefficients.Np);

        // 5. Расчет числа Фурье
        var fourier = Math.Round(
            (1 / -coefficients.MuSquared) * Math.Log(thetaCenter / coefficients.Np),
            2);

        // 6. Расчет времени
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