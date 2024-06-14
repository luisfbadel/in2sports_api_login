namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class UserType : PostgresEntity
    {
        public int Id { get; set; }

        public string? DescriptionType { get; set; }

        public int Status { get; set; }

    }
}
