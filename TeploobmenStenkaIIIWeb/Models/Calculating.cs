namespace HeatTransferCalculator.Models
{
    public class CalculationModel
    {
        // Параметры пластины
        public double Thickness { get; set; }       // Толщина пластины (м)
        public double InitialTemperature { get; set; } // Начальная температура (°C)
        public double EnvironmentTemperature { get; set; } // Температура среды (°C)

        // Теплофизические свойства
        public double Density { get; set; }         // Плотность (кг/м³)
        public double HeatCapacity { get; set; }     // Теплоемкость (Дж/(кг·К))
        public double ThermalConductivity { get; set; } // Теплопроводность (Вт/(м·К))
        public double HeatTransferCoefficient { get; set; } // Коэф. теплоотдачи (Вт/(м²·К))

        // Параметры расчета
        public double? Time { get; set; }          // Время процесса (с) - для прямой задачи
        public double? TargetTemperature { get; set; } // Целевая температура (°C) - для обратной задачи

        // Результаты
        public double CenterTemperature { get; set; }
        public double AverageTemperature { get; set; }
        public double SurfaceTemperature { get; set; }
        public double FourierNumber { get; set; }
        public double BiotNumber { get; set; }
    }
}