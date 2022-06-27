using Newtonsoft.Json;

namespace Project.DTOs
{
    public class WatchlistDTO
    {
        public int IdCompany { get; set; }
        public string Logo { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Sector { get; set; }
        public string Country { get; set; }
        public string Ceo { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
