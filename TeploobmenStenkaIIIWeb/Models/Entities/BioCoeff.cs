using System.ComponentModel.DataAnnotations;

namespace TeploobmenStenkaIIIWeb.Models.Entities
{
    public class BioCoeff
    {
        [Key]
        public int Id { get; set; }

        public double Bio { get; set; }

        public double Mu { get; set; }

        public double MuSquared { get; set; }

        public double Np { get; set; }

        public double Pp {  get; set; }

        public double M {  get; set; }
    }
}
