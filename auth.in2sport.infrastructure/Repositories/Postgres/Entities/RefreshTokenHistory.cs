namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class RefreshTokenHistory : PostgresEntity
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public string Token {  get; set; }

        public string RefreshToken { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ExpirationDate { get; set;}
    }
}
