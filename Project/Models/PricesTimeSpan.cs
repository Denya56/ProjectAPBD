using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class PricesTimeSpan
    {
        [Key]
        public int IdPricesTimeSpan { get; set; }
        public DateTime Date { get; set; }
        public Double Open { get; set; }
        public Double Low { get; set; }
        public Double Close { get; set; }
        public Double High { get; set; }
        public Double Volume { get; set; }
        public override string ToString()
        {
            return $"{Date}, {Open}, {Close}, {Low}, {High}, {Volume}";
        }
    }
}
