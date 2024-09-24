namespace auth.in2sport.application.Services.LoginServices.Requests
{
    public class UpdatePasswodRequest
    {
        public Guid UserId { get; set; }

        public string NewPassword { get; set; }
    }
}
