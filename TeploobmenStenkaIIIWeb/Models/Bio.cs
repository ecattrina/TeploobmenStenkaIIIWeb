namespace HeatTransferCalculator.Models
{
    public class BiotCoefficient
    {
        public int Id { get; set; }
        public double Bi { get; set; }             // Число Био
        public double Mu1 { get; set; }            // Первый корень
        public double Mu1Squared { get; set; }     // Квадрат первого корня
        public double Np { get; set; }             // Коэффициент N для центра
        public double Pp { get; set; }             // Коэффициент P для поверхности
        public double M { get; set; }              // Коэффициент M для средней температуры
    }
}