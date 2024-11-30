namespace auth.in2sport.application.Services.UserServices.Request
{
    public class CreateTicketRequest
    {
        public Guid UserId { get; set; }

        public string Tittle {  get; set; }

        public string Description {  get; set; } 
    }
}
