namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class AgeRange : PostgresEntity
    {
        public int Id { get; set; }

        public int? StartAge { get; set; }

        public int? EndAge { get; set; }

        public string? Description { get; set;}
    }
}
