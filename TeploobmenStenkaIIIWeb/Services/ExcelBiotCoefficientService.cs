using HeatTransferCalculator.Models;
using OfficeOpenXml;

namespace TeploobmenStenkaIIIWeb.Services
{
    public class ExcelBiotCoefficientService
    {
        private readonly List<BiotCoefficient> _coefficients;

        public ExcelBiotCoefficientService(IWebHostEnvironment env)
        {
            var filePath = Path.Combine(env.ContentRootPath, "Таблица.xlsx");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл Excel не найден: {filePath}");

            _coefficients = LoadCoefficients(filePath);
        }

        private List<BiotCoefficient> LoadCoefficients(string filePath)
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

            return coefficients;
        }

        public BiotCoefficient GetCoefficients(double biValue)
        {// Округляем Bi до 2 знаков как в Excel
            biValue = Math.Round(biValue, 2);


            return _coefficients.FirstOrDefault(c => c.Bi == biValue)
                   ?? throw new Exception($"Коэффициенты для Bi={biValue} не найдены");

        }
    }
}