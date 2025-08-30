namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class RecoverPassword : PostgresEntity
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public string Email { get; set; }

        public DateTime RecoverTime { get; set; }

        public string Code { get; set; }
    }
}
