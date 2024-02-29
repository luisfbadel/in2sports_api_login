namespace auth.in2sport.application.Services.UserServices.Response
{
    public class UserFailedException : Exception
    {
        public int StatusCode { get; }

        public UserFailedException() : base()
        {
            StatusCode = 500;
        }

        public UserFailedException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }

        public UserFailedException(string message, Exception innerException, int statusCode = 500) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
