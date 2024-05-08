namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class TypeUser : PostgresEntity
    {
        public int Id { get; set; }

        public string? DescriptionType { get; set; }

        public int Status { get; set; }

    }
}
