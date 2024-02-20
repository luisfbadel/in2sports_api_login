namespace auth.in2sport.application.Services.LoginServices.Response
{
    public class LoginFailedException :  Exception
    {
        public int StatusCode { get; }

        public LoginFailedException() : base()
        {
            StatusCode = 500;
        }

        public LoginFailedException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }

        public LoginFailedException(string message, Exception innerException, int statusCode = 500) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
