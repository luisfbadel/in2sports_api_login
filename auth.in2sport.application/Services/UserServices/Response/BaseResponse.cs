namespace auth.in2sport.application.Services.UserServices.Response
{
    public class BaseResponse<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
