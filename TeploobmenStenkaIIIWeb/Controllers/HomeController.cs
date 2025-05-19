using HeatTransferCalculator.Models;
using Microsoft.AspNetCore.Mvc;
using TeploobmenStenkaIIIWeb.Services;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

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
    public IActionResult DirectProblem(CalculationModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // 1. ������ ������������ ���������������������� (��� � Excel)
            double a = Math.Round(
                model.ThermalConductivity / (model.Density * model.HeatCapacity),
                6); // 6 ������ ����� �������

            // 2. ������ ����� ��� (��� � Excel)
            model.BiotNumber = Math.Round(
                (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
                2); // 2 ����� ��� � Excel

            // 3. ��������� ������������� (������ ������������ Excel)
            var coefficients = _excelService.GetCoefficients(model.BiotNumber);

            // 4. ������ ����� ����� (��� � Excel)
            model.FourierNumber = Math.Round(
                (a * model.Time.Value) / Math.Pow(model.Thickness / 2, 2),
                2); // 2 ����� ��� � Excel

            // 5. ������ ������������ ���������� (������� �� Excel)
            double thetaCenter = Math.Round(
                coefficients.Np * Math.Exp(-coefficients.Mu1Squared * model.FourierNumber),
                4);

            double thetaAverage = Math.Round(
                coefficients.M * Math.Exp(-coefficients.Mu1Squared * model.FourierNumber),
                4);

            double thetaSurface = Math.Round(
                coefficients.Pp * Math.Exp(-coefficients.Mu1Squared * model.FourierNumber),
                4);

            // 6. ������ ���������� (������� � ���������� ��� � Excel)
            model.CenterTemperature = Math.Round(
                model.EnvironmentTemperature +
                (model.InitialTemperature - model.EnvironmentTemperature) * thetaCenter,
                0); // ���������� �� �����

            model.AverageTemperature = Math.Round(
                model.EnvironmentTemperature +
                (model.InitialTemperature - model.EnvironmentTemperature) * thetaAverage,
                0);

            model.SurfaceTemperature = Math.Round(
                model.EnvironmentTemperature +
                (model.InitialTemperature - model.EnvironmentTemperature) * thetaSurface,
                0);


            if (model.FourierNumber <= 0.3 ||
            double.IsNaN(model.CenterTemperature) ||
            double.IsNaN(model.AverageTemperature) ||
            double.IsNaN(model.SurfaceTemperature))
            {
                ModelState.AddModelError("", "������: �������� ������������ ���������� �������. ��������� ��������� ���������.");
                return View(model);
            }

            return View("DirectResult", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"������ �������: {ex.Message}");
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // 1. ������ ������������ ����������������������
            double a = model.ThermalConductivity / (model.Density * model.HeatCapacity);

            // 2. ������ ����� ���
            model.BiotNumber = Math.Round(
                (model.HeatTransferCoefficient * (model.Thickness / 2)) / model.ThermalConductivity,
                2);

            // 3. ��������� �������������
            var coefficients = _excelService.GetCoefficients(model.BiotNumber);

            // 4. ������ ������������ �����������
            double thetaCenter = (model.EnvironmentTemperature - model.TargetTemperature.Value) /
                               (model.EnvironmentTemperature - model.InitialTemperature);

            // 5. ������ ����� �����
            model.FourierNumber = Math.Round(
                (1 / -coefficients.Mu1Squared) * Math.Log(thetaCenter / coefficients.Np),
                2);

            // 6. ������ �������
            model.Time = (model.FourierNumber * Math.Pow(model.Thickness / 2, 2)) / a;

            // ����� �������� �� ������������ �������
            if (!model.Time.HasValue || double.IsNaN(model.Time.Value) || model.Time.Value <= 0)
            {
                throw new Exception("�������� ������������ �������� �������. ��������� ��������� ���������.");
            }

            if (model.FourierNumber <= 0.3 ||
            double.IsNaN(model.CenterTemperature) ||
            double.IsNaN(model.AverageTemperature) ||
            double.IsNaN(model.SurfaceTemperature))
            {
                ModelState.AddModelError("", "������: �������� ������������ ���������� �������. ��������� ��������� ���������.");
                return View(model);
            }

            return View("InverseResult", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"������ �������: {ex.Message}");
            return View(model);
        }
    }
    public IActionResult DirectResult(CalculationModel model)
    {
        return View(model);
    }
}