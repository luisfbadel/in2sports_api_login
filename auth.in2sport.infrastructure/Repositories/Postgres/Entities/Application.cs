namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class Application : PostgresEntity
    {
        public required string Name { get; set; }
        public Organization? Organization { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
