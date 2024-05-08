using auth.in2sport.application.Services.UserServices.Request;
using auth.in2sport.application.Services.UserServices.Response;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;

namespace auth.in2sport.application.Services.UserServices
{
    public interface IUserService
    {
        Task<BaseResponse<List<UserResponse>>> GetUsers(int page, int pageSize);
        Task<BaseResponse<UserResponse>> ActivateUser(Guid id);
        Task<BaseResponse<UserResponse>> InactivateUser(Guid id);
        Task<BaseResponse<UserResponse>> UpdateUser(UpdateUserRequest entity);
        Task<BaseResponse<List<UserResponse>>> GetByFilterAsync(string filter, Guid userId);
        Task<BaseResponse<List<DataRegisteredeUsersResponse>>> GetDataRegisteredUsers(DateTime dateOne, DateTime dateTwo);
        Task<BaseResponse<DataUsersStatusResponse>> GetUsersStatus();
        Task<BaseResponse<List<TypeUser>>> GetTypesUser();

    }
}
