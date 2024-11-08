namespace auth.in2sport.application.Services.UserServices.Request
{
    public class PreapprovalRequest
    {
        public string PayerEmail { get; set; }

        public Guid UserId { get; set; }

        public Guid CourseId { get; set; }

        public string Reason { get; set; }

        public decimal AutoRecurringAmount { get; set; }

        public int Frequency { get; set; }

        public string FrequencyType { get; set; }

        public string CurrencyId { get; set; }
    }
}
