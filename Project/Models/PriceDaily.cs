namespace Project.Models
{
    public class PriceDaily
    {
        public int IdPriceDaily { get; set; }
        public string Status { get; set; }
        public string From { get; set; }
        public string Symbol { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public long Volume { get; set; }
        public float AfterHours { get; set; }
        public int PreMarket { get; set; }
    }
}
