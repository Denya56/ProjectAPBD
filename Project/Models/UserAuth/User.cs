using System.ComponentModel.DataAnnotations;

namespace Project.Models.UserAuth
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        [Required]
        //[RegularExpression(@"^[a-zA-Z0-9\s\_-]+$")]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        //public string Email { get; set; }
        public string Salt { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
