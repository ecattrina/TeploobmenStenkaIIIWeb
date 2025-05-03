using OfficeOpenXml;
using TeploobmenStenkaIIIWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Установка лицензии EPPlus
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Добавление сервисов
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ExcelBiotCoefficientService>();

var app = builder.Build();

// Копирование файла при первом запуске (если его нет)
var excelPath = Path.Combine(app.Environment.ContentRootPath, "Таблица.xlsx");
if (!File.Exists(excelPath))
{
    try
    {
        File.Copy(Path.Combine("Resources", "Таблица.xlsx"), excelPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Не удалось скопировать файл: {ex.Message}");
    }
}

// Остальная конфигурация
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Heat}/{action=Index}/{id?}");

app.Run();