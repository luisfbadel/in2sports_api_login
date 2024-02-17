using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;
using auth.in2sport.infrastructure.Repositories;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace auth.in2sport.application.Services.LoginServices
{
    public class LoginService : ILoginService
    {
        private readonly IBaseRepository<Users> _loginRepository;
        private readonly IConfiguration _config;

        public LoginService(IBaseRepository<Users> loginRepository, IConfiguration config)
        {
            _loginRepository = loginRepository ?? throw new ArgumentNullException(nameof(loginRepository));
            _config = config ?? throw new ArgumentNullException();
        }

        public async Task<BaseResponse<SignInResponse>> SignIn(SignInRequest request)
        {
            var user = await _loginRepository.GetByEmailAsync(request.Email!);
            var response = new BaseResponse<SignInResponse>();
            var tokens = new SignInResponse();

            if (user != null)
            {
                var token = Authorize(user);
                tokens.AuthToken = token;
                byte[] hashedPassword = EncriptPasscode(request.Password!);
                bool validatorPassword = user.Password!.SequenceEqual(hashedPassword);

                if (validatorPassword)
                {
                    response.StatusCode = 200;
                    response.Message = "OK";
                    response.Data = tokens;
                }
                else
                {
                    response.StatusCode = 401;
                    response.Message = "Unauthorized";
                }
            }
            else
            {
                response.StatusCode = 401;
                response.Message = "Unauthorized";
            }
            return response;
        }
        public async Task<BaseResponse<SignUpResponse>> SignUp(SignUpRequest request)
        {
            var user = await _loginRepository.GetByEmailAsync(request.Email!);
            var response = new BaseResponse<SignUpResponse>();

            if (user == null)
            {
                var userEntity = new Users();
                userEntity.Email = request.Email;
                userEntity.Password = EncriptPasscode(request.Password!);
                userEntity.Status = 1;
                userEntity.TypeUser = request.TypeUser;
                userEntity.FirstName = request.FirstName;
                userEntity.SecondName = request.SecondName;
                userEntity.FirstLastname = request.FirstLastname;
                userEntity.SecondLastname = request.SecondLastname;
                userEntity.TypeDocument = request.TypeDocument;
                userEntity.DocumentNumber = request.DocumentNumber;
                userEntity.PhoneNumber = request.PhoneNumber;
                userEntity.Address = request.Address;

                var result = await _loginRepository.CreateAsync(userEntity);


                if (result)
                {
                    response.StatusCode = 201;
                    response.Message = "OK";
                }
                else
                {
                    response.StatusCode = 401;
                    response.Message = "Unauthorized";
                }
            }
            else
            {
                response.StatusCode = 400;
                response.Message = "El usuario ya existe";
            }
            return response;
        }

        private string Authorize(Users user)
        {
            var jwt = _config.GetSection("jwt");

            var claims = new[]
            {

                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("usuario", user.Email!)

                };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                            _config["Jwt:Issuer"],
                            _config["Jwt:Audience"],
                                 claims,
                            expires: DateTime.Now.AddMinutes(60),
                            signingCredentials: credentials);

            return  new JwtSecurityTokenHandler().WriteToken(token);
        }

        private byte[] EncriptPasscode(string password)
        {
            byte[] hashedPassword = Encoding.Unicode.GetBytes(password!);
            return hashedPassword;
        }
    }
}
