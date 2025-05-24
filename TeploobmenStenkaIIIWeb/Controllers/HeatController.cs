using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TeploobmenStenkaIIIWeb.Services;

namespace TeploobmenStenkaIIIWeb.Controllers
{
    public class HeatController : Controller
    {
        private readonly ExcelBiotCoefficientService _dbAccessService;

        public HeatController(ExcelBiotCoefficientService dbAccessService)
        {
            _dbAccessService = dbAccessService;
        }

        public async Task<IActionResult> UserGuide()
        {
            // Получаем первые 15 коэффициентов Био из БД
            var bioCoeffs = await _dbAccessService
                .GetAllCoefficients()
                .OrderBy(x => x.Bio)
                .Take(15)
                .ToListAsync();
            ViewBag.BioCoeffs = bioCoeffs;
            return View();
        }
    }
} 