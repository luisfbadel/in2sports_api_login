using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;

namespace auth.in2sport.application.Services.LoginServices
{
    public interface ILoginService
    {
        Task<BaseResponse<SignInResponse>> SignIn(SignInRequest request);
        Task<BaseResponse<SignUpResponse>> SignUp(SignUpRequest request);
    }
}
