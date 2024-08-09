namespace auth.in2sport.application.Services.LoginServices.Response
{
    public class SignInResponse
    {
        public string? AuthToken { get; set; }

        public string? RefreshToken { get; set; }

        public  UserResponse? user { get; set;}
    }
}
