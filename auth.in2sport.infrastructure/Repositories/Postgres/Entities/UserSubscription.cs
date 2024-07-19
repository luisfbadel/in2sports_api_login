namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class UserSubscription:PostgresEntity
    {
        public int Id { get; set; }

        public Guid? UserId { get; set; }

        public Guid? CourseId { get; set; }

        public int MonthsSubscribed { get; set; }

        public DateTime LastDate { get; set; }
    }
}
