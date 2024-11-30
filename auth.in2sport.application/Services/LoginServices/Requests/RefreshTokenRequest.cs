namespace auth.in2sport.application.Services.LoginServices.Requests
{
    public class RefreshTokenRequest
    {
        public string TokenExpirado { get; set; }

        public string RefreshToken { get; set; }
    }
}
