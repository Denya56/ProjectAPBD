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
    }
}
