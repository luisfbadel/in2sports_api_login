using System.ComponentModel.DataAnnotations;

namespace auth.in2sport.application.Services.LoginServices.Requests
{
    public class SignInRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
