using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;
using auth.in2sport.application.Services.UserServices.Request;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;

namespace auth.in2sport.application.Services.LoginServices
{
    public interface ILoginService
    {
        Task<BaseResponse<SignInResponse>> SignIn(SignInRequest request);
        Task<BaseResponse<SignUpResponse>> SignUp(SignUpRequest request);
        Task<BaseResponse<string>> UserRegistration(List<SignUpRequest> request);
        Task<BaseResponse<SignInResponse>> UpdatePassword(UpdatePasswodRequest request);
        Task<BaseResponse<SignInResponse>> GetRefreshToken(RefreshTokenRequest request);
        Task<BaseResponse<SignInResponse>> ValidateEmail(CodeKeyRequest request);
        Task<BaseResponse<SignInResponse>> RecoverPassword(RecoverPasswordRequest request);
        Task<BaseResponse<SignInResponse>> ValidateCode(CodeKeyRequest request);
    }
}
