﻿using System.ComponentModel.DataAnnotations;

namespace TeploobmenStenkaIIIWeb.Models.InputModels.Direct
{
    public class DirectCalculationModel
    {
        // Параметры пластины
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(0.01, 10.0, ErrorMessage = "Толщина должна быть от 0.01 до 10 м")]
        public double Thickness { get; set; }  // м

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(-273, 3000, ErrorMessage = "Недопустимое значение температуры")]
        public double InitialTemperature { get; set; }  // °C

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(-273, 3000, ErrorMessage = "Недопустимое значение температуры")]
        public double EnvironmentTemperature { get; set; }  // °C

        // Теплофизические свойства
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(1, 20000, ErrorMessage = "Плотность должна быть от 1 до 20000 кг/м³")]
        public double Density { get; set; }  // кг/м³

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(100, 5000, ErrorMessage = "Теплоемкость должна быть от 100 до 5000 Дж/(кг·К)")]
        public double HeatCapacity { get; set; }  // Дж/(кг·К)

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(0.1, 500, ErrorMessage = "Теплопроводность должна быть от 0.1 до 500 Вт/(м·К)")]
        public double ThermalConductivity { get; set; }  // Вт/(м·К)

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(1, 1000, ErrorMessage = "Коэффициент теплоотдачи должен быть от 1 до 1000 Вт/(м²·К)")]
        public double HeatTransferCoefficient { get; set; }  // Вт/(м²·К)

        // Параметры расчета
        [Required(ErrorMessage = "Введите время процесса.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Время должно быть больше нуля.")]
        public double Time { get; set; }  // с

        private double TempTransferCoefficient => ThermalConductivity / (HeatCapacity * Density);
        public double MinTimeInterval => 0.3 * Math.Pow(Thickness / 2, 2) / TempTransferCoefficient;

        [Required(ErrorMessage = "Введите температурный интервал для диаграммы.")]
        public double TimeInterval { get; set; }


        // Результаты

        public List<DirectDiagramData> DiagramData { get; set; } = [];

        public double CenterTemperature { get; set; }
        public double AverageTemperature { get; set; }
        public double SurfaceTemperature { get; set; }
        public double FourierNumber { get; set; }
        public double BiotNumber { get; set; }

        public string? WarningMessage { get; set; }

        public string FormattedThermalDiffusivity => (ThermalConductivity / (Density * HeatCapacity)).ToString("0.000E+00");
        public string FormattedTime
        {
            get
            {
                var ts = TimeSpan.FromSeconds(Time);
                return $"{ts.Hours} ч {ts.Minutes} мин";
            }
        }


    }
}
