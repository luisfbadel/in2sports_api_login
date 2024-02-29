namespace auth.in2sport.application.Services.LoginServices.Response
{
    public class CreateFailedException : Exception
    {
        public int StatusCode { get; }

        public CreateFailedException() : base() 
        {
            StatusCode = 500;
        }

        public CreateFailedException(string message, int statusCode = 500) : base(message) 
        {
            StatusCode = statusCode;
        }

        public CreateFailedException(string message, Exception innerException, int statusCode = 500) : base(message, innerException) 
        { 
            StatusCode = statusCode;    
        }
    }
}
