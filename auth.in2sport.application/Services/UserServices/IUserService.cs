using auth.in2sport.application.Services.UserServices.Response;

namespace auth.in2sport.application.Services.UserServices
{
    public interface IUserService
    {
        Task<BaseResponse<List<UserResponse>>> GetUsers(int page, int pageSize);
        Task<BaseResponse<UserResponse>> ActivateUser(Guid id);
        Task<BaseResponse<UserResponse>> InactivateUser(Guid id);

    }
}
