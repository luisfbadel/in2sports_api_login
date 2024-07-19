namespace auth.in2sport.application.Services.UserServices.Request
{
    public class PreferenceDtoRequest
    {
        public string Title { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public Guid CourseId { get; set; }

        public Guid UserId { get; set; }

    }
}
