namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class ApplicationKey : PostgresEntity
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public Application? Application { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
