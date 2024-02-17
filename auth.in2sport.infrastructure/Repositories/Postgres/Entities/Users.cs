using System.Numerics;

namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class Users : PostgresEntity
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public byte[]? Password { get; set; }
        public string? TypeUser { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? FirstLastname { get; set; }
        public string? SecondLastname { get; set; }
        public int? TypeDocument { get; set; }
        public long? DocumentNumber { get; set; }
        public long? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int Status { get; set; }
    }
}
