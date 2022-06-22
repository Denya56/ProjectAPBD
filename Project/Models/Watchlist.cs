using Project.Models.UserAuth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Watchlist
    {
        [Required]
        public int IdUser { get; set; }
        [Required]
        [ForeignKey("IdUser")]
        public User User { get; set; }
        public int IdCompany { get; set; }
        [Required]
        [ForeignKey("IdCompany")]
        public Company Company { get; set; }
        [Required]
        public string Logo { get; set; }
        [Required]
        public string Symbol { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Sector { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Ceo { get; set; }
    }
}
