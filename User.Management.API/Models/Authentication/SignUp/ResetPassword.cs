using System.ComponentModel.DataAnnotations;

namespace User.Management.API.Models.Authentication.SignUp
{
    public class ResetPassword
    {
        [Required]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirm password do not match.")]
        public string? ConfirmPassword { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
    }
}
