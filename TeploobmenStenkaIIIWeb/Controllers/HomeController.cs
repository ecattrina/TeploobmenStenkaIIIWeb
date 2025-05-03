using HeatTransferCalculator.Models;
using Microsoft.AspNetCore.Mvc;
using TeploobmenStenkaIIIWeb.Services;
using System;
using System.Linq;

public class HeatController : Controller
{
    private readonly ExcelBiotCoefficientService _excelService;

    public HeatController(ExcelBiotCoefficientService excelService)
    {
        _excelService = excelService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DirectProblem()
    {
        return View(new CalculationModel());
    }

    [HttpPost]
    [HttpPost]
    public IActionResult DirectProblem(CalculationModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            // 1. Расчет коэффициента температуропроводности (как в Excel)
            double a = Math.Round(
                model.ThermalConductivity / (model.Density * model.HeatCapacity),
                6); // 6 знаков после запятой

            // 2. Расчет числа Био (как в Excel)
            model.BiotNumber = Math.Round(
                (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
                2); // 2 знака как в Excel

            // 3. Получение коэффициентов (точное соответствие Excel)
            var coefficients = _excelService.GetCoefficients(model.BiotNumber);

            // 4. Расчет числа Фурье (как в Excel)
            model.FourierNumber = Math.Round(
                (a * model.Time.Value) / Math.Pow(model.Thickness / 2, 2),
                2); // 2 знака как в Excel

            // 5. Расчет безразмерных температур (формулы из Excel)
            double thetaCenter = Math.Round(
                coefficients.Np * Math.Exp(-coefficients.Mu1Squared * model.FourierNumber),
                4);

            double thetaAverage = Math.Round(
                coefficients.M * Math.Exp(-coefficients.Mu1Squared * model.FourierNumber),
                4);

            double thetaSurface = Math.Round(
                coefficients.Pp * Math.Exp(-coefficients.Mu1Squared * model.FourierNumber),
                4);

            // 6. Расчет температур (формулы и округление как в Excel)
            model.CenterTemperature = Math.Round(
                model.EnvironmentTemperature +
                (model.InitialTemperature - model.EnvironmentTemperature) * thetaCenter,
                0); // Округление до целых

            model.AverageTemperature = Math.Round(
                model.EnvironmentTemperature +
                (model.InitialTemperature - model.EnvironmentTemperature) * thetaAverage,
                0);

            model.SurfaceTemperature = Math.Round(
                model.EnvironmentTemperature +
                (model.InitialTemperature - model.EnvironmentTemperature) * thetaSurface,
                0);

            return View("Result", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка расчета: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult InverseProblem()
    {
        return View(new CalculationModel());
    }
    [HttpPost]
    public IActionResult InverseProblem(CalculationModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            // 1. Расчет коэффициента температуропроводности
            double a = model.ThermalConductivity / (model.Density * model.HeatCapacity);

            // 2. Расчет числа Био
            model.BiotNumber = Math.Round(
                (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
                2);

            // 3. Получение коэффициентов
            var coefficients = _excelService.GetCoefficients(model.BiotNumber);

            // 4. Расчет безразмерной температуры
            double thetaCenter = (model.TargetTemperature.Value - model.EnvironmentTemperature) /
                               (model.InitialTemperature - model.EnvironmentTemperature);

            // 5. Расчет числа Фурье
            model.FourierNumber = Math.Round(
                (1 / -coefficients.Mu1Squared) * Math.Log(thetaCenter / coefficients.Np),
                2);

            // 6. Расчет времени
            model.Time = (model.FourierNumber * Math.Pow(model.Thickness / 2, 2)) / a;

            return View("InverseResult", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка расчета: {ex.Message}");
            return View(model);
        }
    }
    public IActionResult Result(CalculationModel model)
    {
        return View(model);
    }
}