using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;
using auth.in2sport.infrastructure.Repositories;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using AutoMapper;
using MercadoPago.Resource.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace auth.in2sport.application.Services.LoginServices
{
    public class LoginService : ILoginService
    {

        #region Private Properties

        /// <summary>
        ///  Instance of the Base Repository
        ///  Instance of the Condiguration
        ///  Instance of the Base Mapper
        /// </summary>
        private readonly IBaseRepository<Users> _loginRepository;
        private readonly IBaseRepository<RefreshTokenHistory> _refreshTokenHistoryRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor

        /// <summary>
        /// Defines constructor
        /// </summary>
        /// <param name="loginRepository"></param>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LoginService(IBaseRepository<Users> loginRepository, IBaseRepository<RefreshTokenHistory> refreshTokenHistoryRepository, IConfiguration config, IMapper mapper)
        {
            _loginRepository = loginRepository ?? throw new ArgumentNullException(nameof(loginRepository));
            _refreshTokenHistoryRepository = refreshTokenHistoryRepository ?? throw new ArgumentNullException(nameof(refreshTokenHistoryRepository));
            _config = config ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        #endregion

        public async Task<BaseResponse<SignInResponse>> SignIn(SignInRequest request)
        {
            var response = new BaseResponse<SignInResponse>();
            var tokens = new SignInResponse();

            try
            {
                var user = await _loginRepository.GetByEmailAsync(request.Email!);

                if (user == null)
                {
                    throw new LoginFailedException("El usuario no existe");
                }
                try
                {
                    //byte[] hashedPassword = EncriptPasscode("k12345");
                    byte[] dataBytes = Convert.FromBase64String(request.Password);
                    bool validatorPassword = user!.Password!.SequenceEqual(dataBytes);
                    if (!validatorPassword)
                    {
                        response.StatusCode = 401;
                        response.Message = "Unauthorized";
                        return response;
                    }

                    string token = Authorize(user);
                    string refreshTokenCreated = GenerarRefreshToken();
                    tokens.user = _mapper.Map<UserResponse>(user);
                    tokens.AuthToken = token;
                    tokens.RefreshToken = refreshTokenCreated;

                    var result = await KeepRefreshTokenHistory(user, token, refreshTokenCreated);

                    response.StatusCode = 200;
                    response.Message = "OK";
                    response.Data = tokens;
                    
                    return response;
                }
                catch (Exception ex)
                {
                    throw new LoginFailedException($"Error durante la autorización: {ex.Message}", 500);
                }
            }
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error durante la obtención del usuario: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<SignUpResponse>> SignUp(SignUpRequest request)
        {
            var response = new BaseResponse<SignUpResponse>();
            var tokens = new SignUpResponse();

            try
            {
                using (var transaction = await _loginRepository.BeginTransactionAsync())
                {
                    try
                    {
                        var user = await _loginRepository.GetByEmailAsync(request.Email!);

                        if (user != null)
                        {
                            throw new CreateFailedException("El usuario ya existe");
                        }
                        DateTime utcNow = DateTime.UtcNow;
                        DateTime localDate = utcNow.AddHours(+5).Date;

                        var userEntity = new Users
                        {
                            Email = request.Email,
                            Password = EncriptPasscode(request.Password!),
                            Status = (int)request.Status,
                            TypeUser = request.TypeUser,
                            FirstName = request.FirstName,
                            SecondName = request.SecondName,
                            FirstLastname = request.FirstLastname,
                            SecondLastname = request.SecondLastname,
                            TypeDocument = request.TypeDocument,
                            DocumentNumber = request.DocumentNumber,
                            PhoneNumber = request.PhoneNumber,
                            Address = request.Address,
                            CreationDate = localDate,
                            PasswordValidation = (int)request.PasswordValidation,
                            Birthdate = (DateTime)request.Birthdate.ToUniversalTime(),
                        };

                        var result = await _loginRepository.CreateAsync(userEntity);

                        if (!result)
                        {
                            throw new CreateFailedException("Error al crear el usuario");
                        }

                        await transaction.CommitAsync();

                        var token = Authorize(userEntity);
                        tokens.user = _mapper.Map<UserResponse>(userEntity);
                        tokens.AuthToken = token;

                        response.StatusCode = 201;
                        response.Message = "OK";
                        response.Data = tokens;
                        return response;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new CreateFailedException($"Error durante la creación del usuario: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error general: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<string>> UserRegistration(List<SignUpRequest> request)
        {
            var response = new BaseResponse<string>();

            try
            {
                using (var transaction = await _loginRepository.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var userRequest in request)
                        {
                            
                            var user = await _loginRepository.GetByEmailAsync(userRequest.Email!);
                            DateTime utcNow = DateTime.UtcNow;
                            DateTime localDate = utcNow.AddHours(+5).Date;

                            if (user == null)
                            {
                                var userEntity = new Users
                                {
                                    Email = userRequest.Email,
                                    Password = EncriptPasscode(userRequest.Password!),
                                    Status = (int)userRequest.Status,
                                    TypeUser = userRequest.TypeUser,
                                    FirstName = userRequest.FirstName,
                                    SecondName = userRequest.SecondName,
                                    FirstLastname = userRequest.FirstLastname,
                                    SecondLastname = userRequest.SecondLastname,
                                    TypeDocument = userRequest.TypeDocument,
                                    DocumentNumber = userRequest.DocumentNumber,
                                    PhoneNumber = userRequest.PhoneNumber,
                                    Address = userRequest.Address,
                                    CreationDate = localDate,
                                    PasswordValidation = (int)userRequest.PasswordValidation,
                                    Birthdate = userRequest.Birthdate
                                };

                                var result = await _loginRepository.CreateAsync(userEntity);

                                if (!result)
                                {
                                    throw new CreateFailedException("Error al crear el usuario");
                                }
                            }
                        }

                        await transaction.CommitAsync();

                        response.StatusCode = 201;
                        response.Message = "OK";
                        response.Data = "Registro exitoso";
                        return response;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new CreateFailedException($"Error durante la creación del usuario: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error general: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<SignInResponse>> UpdatePassword(Guid userId, string newPassword)
        {
            var response = new BaseResponse<SignInResponse>();
            var userResponse = new SignInResponse();

            try
            {
                var user = await _loginRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    throw new LoginFailedException("El usuario no existe");
                }
                try
                {
                    //byte[] hashedPassword = EncriptPasscode("k12345");
                    byte[] dataBytes = Convert.FromBase64String(newPassword);

                    user.Password = dataBytes;
                    user.PasswordValidation = 0;

                    var result = await _loginRepository.UpdateAsync(user);

                    if (!result)
                    {
                        throw new LoginFailedException("El usuario no existe");
                    }
                    userResponse.user = _mapper.Map<UserResponse>(user);
                    userResponse.AuthToken = "";

                    response.StatusCode = 200;
                    response.Message = "OK";
                    response.Data = userResponse;

                    return response;
                }
                catch (Exception ex)
                {
                    throw new LoginFailedException($"Error durante la actualizacion: {ex.Message}", 500);
                }
            }
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error durante la actualizacion: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<SignInResponse>> GetRefreshToken(RefreshTokenRequest request)
        {
            var response = new BaseResponse<SignInResponse>();
            var tokens = new SignInResponse();

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenExpiradoSupposedly = tokenHandler.ReadJwtToken(request.TokenExpirado);

                if (tokenExpiradoSupposedly.ValidTo > DateTime.UtcNow)
                {
                    response.StatusCode = 400;
                    response.Message = "El token no ha expirado";

                    return response;
                }

                string idUsuario = tokenExpiradoSupposedly.Claims.First(x =>
                    x.Type == JwtRegisteredClaimNames.NameId).Value.ToString();


                var refreshTokenFinded = await _refreshTokenHistoryRepository.GetByFilterAsync(entity => entity.UserId == Guid.Parse(idUsuario));

                var userRefrechtoken = refreshTokenFinded[0];

                if (userRefrechtoken.Token != request.TokenExpirado || userRefrechtoken.RefreshToken != request.RefreshToken) {
                    response.StatusCode = 400;
                    response.Message = "No existe refresh token";

                    return response;
                }

                var user = await _loginRepository.GetByIdAsync(userRefrechtoken.UserId);
                var refreshTokenCreated = GenerarRefreshToken();
                var token = Authorize(user);

                tokens.user = _mapper.Map<UserResponse>(user);
                tokens.AuthToken = token;
                tokens.RefreshToken = refreshTokenCreated;

                var result = await KeepRefreshTokenHistory(user, token, refreshTokenCreated);

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = tokens;

                return response;
            }

            
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error durante la obtención del usuario: {ex.Message}", 500);
            }
        }

        #region Private Methods

        private string Authorize(Users user)
        {
            try
            {
                var claims = new ClaimsIdentity();
                claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    SigningCredentials = credentials

                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenConfig = tokenHandler.CreateToken(token);

                return new JwtSecurityTokenHandler().WriteToken(tokenConfig);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Se produjo una excepción de argumento nulo: {ex.Message}");
                throw;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Se produjo una excepción de argumento inválido: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Se produjo una excepción no manejada: {ex.Message}");
                throw;
            }
        }

        private byte[] EncriptPasscode(string password)
        {
            try
            {
                byte[] hashedPassword = Encoding.UTF8.GetBytes(password!);

                return hashedPassword;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Error: La cadena de contraseña es nula. {ex.Message}");
                throw;
            }
            catch (EncoderFallbackException ex)
            {
                Console.WriteLine($"Error: Problema con la codificación de caracteres. {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado durante la encriptación de la contraseña. {ex.Message}");
                throw;
            }
        }

        private string GenerarRefreshToken()
        {
            var byteArray = new byte[64];
            var refreshToken = "";

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(byteArray);
                refreshToken = Convert.ToBase64String(byteArray);
            }
            return refreshToken;
        }

        private async Task<bool> KeepRefreshTokenHistory(Users user, string token, string refreshToken)
        {

            var refreshTokenFinded = await _refreshTokenHistoryRepository.GetByFilterAsync(entity => entity.UserId == user.Id);

            bool result;
            if (refreshTokenFinded.Count == 0)
            {
                var refreshTokenHistory = new RefreshTokenHistory
                {
                    UserId = user.Id,
                    Token = token,
                    RefreshToken = refreshToken,
                    CreationDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(10)
                };

                result = await _refreshTokenHistoryRepository.CreateAsync(refreshTokenHistory);
            }
            else
            {
                var userRefreshToken = refreshTokenFinded[0];

                userRefreshToken.Token = token;
                userRefreshToken.RefreshToken = refreshToken;
                userRefreshToken.CreationDate = DateTime.UtcNow;
                userRefreshToken.ExpirationDate = DateTime.UtcNow.AddMinutes(10);

                result = await _refreshTokenHistoryRepository.UpdateAsync(userRefreshToken);
            }
            return result;
        }

        #endregion
    }
}
