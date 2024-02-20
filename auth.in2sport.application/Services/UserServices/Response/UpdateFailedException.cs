namespace auth.in2sport.application.Services.UserServices.Response
{
    public class UpdateFailedException : Exception
    {
        public int StatusCode { get; }

        public UpdateFailedException() : base() 
        {
            StatusCode = 500;
        }

        public UpdateFailedException(string message, int statusCode = 500) : base( message) 
        {
            StatusCode = statusCode;
        }

        public UpdateFailedException(string message, Exception innerException, int statusCode = 500) : base(message, innerException) 
        {
            StatusCode = statusCode;
        }

    }
}
