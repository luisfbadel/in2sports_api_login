using System.ComponentModel.DataAnnotations;

namespace auth.in2sport.application.Services.UserServices.Request
{
    public class CreateUserRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? TypeUser { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? SecondName { get; set; }
        [Required]
        public string? FirstLastname { get; set; }
        [Required]
        public string? SecondLastname { get; set; }
        [Required]
        public int? TypeDocument { get; set; }
        [Required]
        public long? DocumentNumber { get; set; }
        [Required]
        public long? PhoneNumber { get; set; }
        [Required]
        public string? Address { get; set; }
    }
}
