using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.application.Services.LoginServices.Requests
{
    public class SignUpRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public int? TypeUser { get; set; }

        [Required]
        public string? FirstName { get; set; }

        public string? SecondName { get; set; }

        [Required]
        public string? FirstLastname { get; set; }

        [Required]
        public string? SecondLastname { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }

        [Required]
        public int? TypeDocument { get; set; }

        [Required]
        public long? DocumentNumber { get; set; }

        [Required]
        public long? PhoneNumber { get; set; }

        [Required]
        public string? Departament { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public int? Status { get; set; }

        [Required]
        public int? PasswordValidation { get; set; }

        public string? InstitutionName { get; set; }

        [Required]
        public int? EmailValidation { get; set; }

        [Required]
        public bool? AcceptedConditions { get; set; }
    }
}
