using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.application.Services.LoginServices
{
    public interface ILoginService
    {
        Task<BaseResponse<SignInResponse>> SignIn(SignInRequest request);
        Task<BaseResponse<SignUpResponse>> SignUp(SignUpRequest request);
    }
}
