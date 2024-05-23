using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;
using auth.in2sport.infrastructure.Repositories;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using AutoMapper;
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

        #region Private Properties

        /// <summary>
        ///  Instance of the Base Repository
        ///  Instance of the Condiguration
        ///  Instance of the Base Mapper
        /// </summary>
        private readonly IBaseRepository<Users> _loginRepository;
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
        public LoginService(IBaseRepository<Users> loginRepository, IConfiguration config, IMapper mapper)
        {
            _loginRepository = loginRepository ?? throw new ArgumentNullException(nameof(loginRepository));
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
                    var token = Authorize(user);
                    tokens.user = _mapper.Map<UserResponse>(user);
                    tokens.AuthToken = token;

                    //byte[] hashedPassword = EncriptPasscode("k12345");
                    byte[] dataBytes = Convert.FromBase64String(request.Password);
                    bool validatorPassword = user!.Password!.SequenceEqual(dataBytes);

                    if (!validatorPassword)
                    {
                        response.StatusCode = 401;
                        response.Message = "Unauthorized";
                        return response;
                    }
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
                            CreationDate = DateTime.UtcNow.Date,
                            PasswordValidation = 0
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

                            if (user != null)
                            {
                                throw new CreateFailedException("El usuario ya existe");
                            }
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
                                CreationDate = DateTime.UtcNow.Date,
                                PasswordValidation = 1
                            };

                            var result = await _loginRepository.CreateAsync(userEntity);

                            if (!result)
                            {
                                throw new CreateFailedException("Error al crear el usuario");
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
                    user.CreationDate = DateTime.UtcNow.Date;

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

        #region Private Methods

        private string Authorize(Users user)
        {
            try
            {
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

                return new JwtSecurityTokenHandler().WriteToken(token);
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

        #endregion
    }
}
