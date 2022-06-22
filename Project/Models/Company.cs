using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Company
    {
        [Key]
        public int IdCompany { get; set; }
        public string? Logo { get; set; }
        public string? Cik { get; set; }
        public string? Bloomberg { get; set; }
        public string? Figi { get; set; }
        public string? Lei { get; set; }
        public string? Sic { get; set; }
        public string? Country { get; set; }
        public string? Industry { get; set; }
        public string? Sector { get; set; }
        public string? MarketCap { get; set; }
        public string? EmployeesAmount { get; set; }
        public string? Phone { get; set; }
        public string? Ceo { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public string? Exchange { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? ExchangeSymbol { get; set; }
        public string? HqAddress { get; set; }
        public string? HqState { get; set; }
        public string? HqCountry { get; set; }
        public string? Type { get; set; }
        public string? Updated { get; set; }
        /*public List<string>? Tags { get; set; }
        public List<string> Similar { get; set; }*/
        public bool Active { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    
}