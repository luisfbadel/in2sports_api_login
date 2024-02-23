namespace auth.in2sport.application.Services.LoginServices.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }

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

        public int status { get; set; }
    }
}
