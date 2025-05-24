using HeatTransferCalculator.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeploobmenStenkaIIIWeb.Models.Entities;

namespace TeploobmenStenkaIIIWeb.Services
{
    public class ExcelBiotCoefficientService
    {
        //private readonly List<BiotCoefficient> _coefficients;
        private readonly AppDbContext _dbContext;

        public ExcelBiotCoefficientService(/*IWebHostEnvironment env, */AppDbContext dbContext)
        {
            /*var filePath = Path.Combine(env.ContentRootPath, "Таблица.xlsx");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл Excel не найден: {filePath}");*/

            _dbContext = dbContext;
            //_coefficients = LoadCoefficients(filePath);
        }

        /*private List<BiotCoefficient> LoadCoefficients(string filePath)
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets["ТАБЛИЦА"] ??
                           throw new Exception("Лист 'ТАБЛИЦА' не найден");

            var coefficients = new List<BiotCoefficient>();

            for (int row = 5; row <= worksheet.Dimension.End.Row; row++)
            {
                // Проверяем, что строка не пустая
                if (worksheet.Cells[row, 1].Value == null) continue;

                coefficients.Add(new BiotCoefficient
                {
                    Bi = Convert.ToDouble(worksheet.Cells[row, 1].Value),
                    Mu1 = Convert.ToDouble(worksheet.Cells[row, 2].Value),
                    Mu1Squared = Convert.ToDouble(worksheet.Cells[row, 3].Value),
                    Np = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                    Pp = Convert.ToDouble(worksheet.Cells[row, 5].Value),
                    M = Convert.ToDouble(worksheet.Cells[row, 6].Value)
                });
            }

            if (coefficients.Count == 0)
                throw new Exception("Не удалось загрузить коэффициенты из Excel");

            if (_dbContext == null)
                return coefficients;

            var count = 0;
            foreach (var part in coefficients
                .Select(x => new BioCoeff
                    {
                        Bio = x.Bi,
                        Mu = x.Mu1,
                        MuSquared = x.Mu1Squared,
                        Np = x.Np,
                        Pp = x.Pp,
                        M = x.M
                    })
                .Chunk(200))
            {
                count += part.Length;
                _dbContext.BioCoeffs.AddRange(part);
                _dbContext.SaveChanges();
                Console.WriteLine($"{count}/10000 записей готово");
            }

            return coefficients;
        }*/

        public async Task<BioCoeff> GetCoefficients(double biValue)
        {
            // Округляем Bi до 2 знаков как в Excel
            biValue = Math.Round(biValue, 2);

            return await _dbContext.BioCoeffs.FirstOrDefaultAsync(c => c.Bio == biValue)
                   ?? throw new Exception($"Коэффициенты для Bi={biValue} не найдены");

        }

        public IQueryable<BioCoeff> GetAllCoefficients()
        {
            return _dbContext.BioCoeffs;
        }
    }
}