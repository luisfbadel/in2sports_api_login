namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class PaymentRecordInstitution : PostgresEntity
    {
        public int Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid InstitutionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
