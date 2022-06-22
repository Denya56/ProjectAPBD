using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.UserAuth
{
    public class UserDTO
    {
        [StringLength(15, ErrorMessage = "Login is too long. 15 characters max")]
        [MinLength(3, ErrorMessage = "Login is too short. 3 characters min")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Login can only contain lower or uppercase letters, numbers and/or _ -")]
        public string Login { get; set; }
        //[RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$", ErrorMessage = "Minimum eight characters, at least one upper case English letter, one lower case English letter, one number and one special character")]
        public string Password { get; set; }
        //public string Email { get; set; }
    }
}
