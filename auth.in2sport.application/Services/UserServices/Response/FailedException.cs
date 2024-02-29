namespace auth.in2sport.application.Services.UserServices.Response
{
    public class FailedException : Exception
    {
        public int StatusCode { get; }

        public FailedException() : base()
        {
            StatusCode = 500;
        }

        public FailedException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }

        public FailedException(string message, Exception innerException, int statusCode = 500) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
