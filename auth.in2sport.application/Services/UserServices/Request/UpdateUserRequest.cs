using System.ComponentModel.DataAnnotations;

namespace auth.in2sport.application.Services.UserServices.Request
{
    public class UpdateUserRequest
    {
        [Required]
        public Guid Id { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? TypeUser { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? FirstLastname { get; set; }
        public string? SecondLastname { get; set; }
        public int? TypeDocument { get; set; }
        public long? DocumentNumber { get; set; }
        public long? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
