namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class SubscriptionVerification : PostgresEntity
    {
        public int Id { get; set; }

        public string SubscriptionId { get; set; }

        public Guid UserId { get; set; }

        public Guid CourseId { get; set; }
    }
}
