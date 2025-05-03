using HeatTransferCalculator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HeatTransferCalculator.Controllers
{
    public class HeatController : Controller
    {
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

            // Здесь будет логика расчета
            // Пока просто вернем модель
            return View("Result", model);
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

            // Здесь будет логика расчета
            return View("Result", model);
        }

        public IActionResult Result(CalculationModel model)
        {
            return View(model);
        }
    }
}