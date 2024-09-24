namespace auth.in2sport.application.Services.UserServices.Request
{
    public class PreapprovalRequest
    {
        public string PayerEmail { get; set; }
        public string Reason { get; set; }
        public decimal AutoRecurringAmount { get; set; }
        public int Frequency { get; set; }
        public string FrequencyType { get; set; } // "months", "days", etc.
        public string CurrencyId { get; set; } // "ARS", "USD", etc.
    }
}
