using OfficeOpenXml;
using TeploobmenStenkaIIIWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� �������� EPPlus
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// ���������� ��������
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ExcelBiotCoefficientService>();

var app = builder.Build();

// ����������� ����� ��� ������ ������� (���� ��� ���)
var excelPath = Path.Combine(app.Environment.ContentRootPath, "�������.xlsx");
if (!File.Exists(excelPath))
{
    try
    {
        File.Copy(Path.Combine("Resources", "�������.xlsx"), excelPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"�� ������� ����������� ����: {ex.Message}");
    }
}

// ��������� ������������
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