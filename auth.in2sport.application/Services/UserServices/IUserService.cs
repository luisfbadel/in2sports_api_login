using auth.in2sport.application.Services.UserServices.Request;
using auth.in2sport.application.Services.UserServices.Response;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.application.Services.UserServices
{
    public interface IUserService
    {
        Task<BaseResponse<List<UserResponse>>> GetUsers(int page, int pageSize);
        Task<BaseResponse<UserResponse>> ActivateUser(Guid id);
        Task<BaseResponse<UserResponse>> InactivateUser(Guid id);

    }
}
