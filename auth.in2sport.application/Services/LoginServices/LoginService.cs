using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.LoginServices.Response;
using auth.in2sport.infrastructure.Repositories;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using AutoMapper;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Text;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using auth.in2sport.application.Services.UserServices.Request;
using System.Text.RegularExpressions;

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
        private readonly IBaseRepository<RecoverPassword> _recoverPassword;
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
        public LoginService(IBaseRepository<Users> loginRepository, IBaseRepository<RefreshTokenHistory> refreshTokenHistoryRepository, IBaseRepository<RecoverPassword> refreshRecoverPassowrd, IConfiguration config, IMapper mapper)
        {
            _loginRepository = loginRepository ?? throw new ArgumentNullException(nameof(loginRepository));
            _refreshTokenHistoryRepository = refreshTokenHistoryRepository ?? throw new ArgumentNullException(nameof(refreshTokenHistoryRepository));
            _recoverPassword = refreshRecoverPassowrd ?? throw new ArgumentNullException(nameof(refreshRecoverPassowrd));
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
                    response.StatusCode = 400;
                    response.Message = "El usuario no existe";
                    return response;
                }
                try
                {
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

                    var result = await KeepRefreshTokenHistory(user, token, refreshTokenCreated);

                    tokens.user = _mapper.Map<UserResponse>(user);
                    tokens.AuthToken = token;
                    tokens.RefreshToken = refreshTokenCreated;


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
            var signUpResponse = new SignUpResponse();

            try
            {
                using (var transaction = await _loginRepository.BeginTransactionAsync())
                {
                    try
                    {
                        var user = await _loginRepository.GetByEmailAsync(request.Email!);

                        if (user != null)
                        {
                            response.StatusCode = 400;
                            response.Message = "El usuario ya existe";

                            return response;
                        }

                        var resultDocumenTNumber = await _loginRepository.GetByFilterAsync(entity => entity.DocumentNumber == request.DocumentNumber);

                        if (resultDocumenTNumber.Count > 0)
                        {
                            response.StatusCode = 400;
                            response.Message = "La cedula ya existe";

                            return response;
                        }
                        DateTime localDate = DateTime.UtcNow.AddHours(-5);

                        var tokenConfirmation = GenerateSecureToken();

                        var userEntity = new Users
                        {
                            Email = RemoveAllSpaces(request.Email),
                            Password = EncriptPasscode(request.Password!),
                            Status = (int)request.Status,
                            TypeUser = request.TypeUser,
                            FirstName = RemoveAllSpaces(request.FirstName),
                            SecondName = RemoveAllSpaces(request.SecondName),
                            FirstLastname = RemoveAllSpaces(request.FirstLastname),
                            SecondLastname = RemoveAllSpaces(request.SecondLastname),
                            TypeDocument = request.TypeDocument,
                            DocumentNumber = long.Parse(KeepOnlyDigits(request.DocumentNumber)),
                            PhoneNumber = long.Parse(KeepOnlyDigits(request.PhoneNumber)),
                            Address = request.Address,
                            CreationDate = localDate,
                            PasswordValidation = (int)request.PasswordValidation,
                            Birthdate = (DateTime)request.Birthdate.ToUniversalTime(),
                            InstitutionName = request.InstitutionName,
                            EmailValidation = (int)request.EmailValidation,
                            TokenConfirmation = tokenConfirmation,
                            Departament = request.Departament,
                            City = request.City,
                            AcceptedConditions = request.AcceptedConditions
                        };

                        var resultSendEmail = await SendEmailRegister(userEntity, tokenConfirmation);
                        if (!resultSendEmail)
                        {
                            throw new CreateFailedException("Error al enviar correo", 400);
                        }

                        var result = await _loginRepository.CreateAsync(userEntity);

                        if (!result)
                        {
                            throw new CreateFailedException("Error al crear el usuario");
                        }

                        await transaction.CommitAsync();

                        signUpResponse.user = _mapper.Map<UserResponse>(userEntity);

                        response.StatusCode = 200;
                        response.Message = "OK";
                        response.Data = signUpResponse;
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
                            DateTime localDate = DateTime.UtcNow.AddHours(-5);

                            if (user == null)
                            {
                                var userEntity = new Users
                                {
                                    Email = RemoveAllSpaces(userRequest.Email),
                                    Password = EncriptPasscode(userRequest.Password!),
                                    Status = (int)userRequest.Status,
                                    TypeUser = userRequest.TypeUser,
                                    FirstName = RemoveAllSpaces(userRequest.FirstName),
                                    SecondName = RemoveAllSpaces(userRequest.SecondName),
                                    FirstLastname = RemoveAllSpaces(userRequest.FirstLastname),
                                    SecondLastname = RemoveAllSpaces(userRequest.SecondLastname),
                                    TypeDocument = userRequest.TypeDocument,
                                    DocumentNumber = long.Parse(KeepOnlyDigits(userRequest.DocumentNumber)),
                                    PhoneNumber = long.Parse(KeepOnlyDigits(userRequest.PhoneNumber)),
                                    Address = userRequest.Address,
                                    CreationDate = localDate,
                                    PasswordValidation = (int)userRequest.PasswordValidation,
                                    Birthdate = userRequest.Birthdate,
                                    Departament = userRequest.Departament,
                                    City = userRequest.City,
                                };

                                var result = await _loginRepository.CreateAsync(userEntity);

                                if (!result)
                                {
                                    throw new CreateFailedException("Error al crear el usuario");
                                }
                            }
                        }

                        await transaction.CommitAsync();

                        response.StatusCode = 200;
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

        public async Task<BaseResponse<SignInResponse>> UpdatePassword(UpdatePasswodRequest request)
        {
            var response = new BaseResponse<SignInResponse>();
            var userResponse = new SignInResponse();

            try
            {
                var user = await _loginRepository.GetByIdAsync(request.UserId);

                if (user == null)
                {
                    throw new LoginFailedException("El usuario no existe");
                }
                try
                {
                    byte[] dataBytes = Convert.FromBase64String(request.NewPassword);

                    user.Password = dataBytes;
                    user.CreationDate = user.CreationDate.ToUniversalTime();
                    user.Birthdate = user.Birthdate.ToUniversalTime();
                    user.PasswordValidation = 0;

                    var result = await _loginRepository.UpdateAsync(user);

                    if (!result)
                    {
                        throw new LoginFailedException("El usuario no existe");
                    }
                    userResponse.user = _mapper.Map<UserResponse>(user);

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

                if (userRefrechtoken.Token != request.TokenExpirado || userRefrechtoken.RefreshToken != request.RefreshToken)
                {
                    response.StatusCode = 400;
                    response.Message = "No existe refresh token";

                    return response;
                }

                var user = await _loginRepository.GetByIdAsync(Guid.Parse(idUsuario));
                var token = Authorize(user);
                var refreshTokenCreated = GenerarRefreshToken();

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

        public async Task<BaseResponse<SignInResponse>> ValidateEmail(CodeKeyRequest request)
        {
            var response = new BaseResponse<SignInResponse>();
            var data = new SignInResponse();

            try
            {
                var user = await _loginRepository.GetByIdAsync(Guid.Parse(request.UserId));

                if (user == null)
                {
                    throw new LoginFailedException("El usuario no existe");
                }

                if (user.TokenConfirmation == request.CodeKey)
                {
                    user.CreationDate = user.CreationDate.ToUniversalTime();
                    user.Birthdate = user.Birthdate.ToUniversalTime();
                    user.Status = 1;
                    user.TokenConfirmation = null;
                    user.EmailValidation = 0;

                    var result = await _loginRepository.UpdateAsync(user);

                    data.user = _mapper.Map<UserResponse>(user);
                    data.AuthToken = "";
                    data.RefreshToken = "";

                    response.StatusCode = 200;
                    response.Message = "OK";
                    response.Data = data;
                }
                else
                {
                    data.user = _mapper.Map<UserResponse>(user);
                    data.AuthToken = "";
                    data.RefreshToken = "";

                    response.StatusCode = 400;
                    response.Message = "El codigo no es valido";
                    response.Data = data;
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error durante la actualizacion: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<SignInResponse>> RecoverPassword(RecoverPasswordRequest request)
        {
            var response = new BaseResponse<SignInResponse>();
            var data = new SignInResponse();

            try
            {
                var user = await _loginRepository.GetByEmailAsync(request.Email);
                var currentlyHour = DateTime.UtcNow;

                if (user == null)
                {
                    response.StatusCode = 400;
                    response.Message = "El usuario no existe";
                    response.Data = data;

                    return response;
                }

                var recoverEmailExist = await _recoverPassword.GetByFilterAsync(entity => entity.UserId == user.Id);

                var tokenConfirmation = "";

                if (recoverEmailExist.Count == 0)
                {
                    tokenConfirmation = GenerateSecureToken();

                    var recoverPassword = new RecoverPassword
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        RecoverTime = currentlyHour.AddMinutes(5),
                        Code = tokenConfirmation
                    };

                    var result = await _recoverPassword.CreateAsync(recoverPassword);

                    var resultSendEmail = await SendEmailRecoverPassword(user, tokenConfirmation);
                    if (!resultSendEmail)
                    {
                        throw new CreateFailedException("Error al enviar correo", 400);
                    }

                } else { 
                    if(DateTime.UtcNow < recoverEmailExist[0].RecoverTime)
                    {
                        response.StatusCode = 400;
                        response.Message = "Para generar un nuevo codigo debe esperar 5 min";

                        return response;
                    } else
                    {
                        tokenConfirmation = GenerateSecureToken();

                        var newRecoverPassword = recoverEmailExist[0];

                        newRecoverPassword.RecoverTime = currentlyHour.AddMinutes(5);
                        newRecoverPassword.Code = tokenConfirmation;
                            
                        var result = await _recoverPassword.UpdateAsync(newRecoverPassword);

                        var resultSendEmail = await SendEmailRecoverPassword(user, tokenConfirmation);
                        if (!resultSendEmail)
                        {
                            throw new CreateFailedException("Error al enviar correo", 400);
                        }
                    }
                }

                data.user = _mapper.Map<UserResponse>(user);
                data.AuthToken = "";
                data.RefreshToken = "";

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = data;

                return response;
            }
            catch (Exception ex)
            {
                throw new LoginFailedException($"Error durante la actualizacion: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<SignInResponse>> ValidateCode(CodeKeyRequest request)
        {
            var response = new BaseResponse<SignInResponse>();
            var data = new SignInResponse();

            try
            {
                var recoverPasswordExist = await _recoverPassword.GetByFilterAsync(entity => entity.UserId == Guid.Parse(request.UserId));
                var recoverPassword = recoverPasswordExist[0];
                var user = await _loginRepository.GetByIdAsync(Guid.Parse(request.UserId));

                if (DateTime.Now < recoverPassword.RecoverTime) {
                    if (recoverPassword.Code != request.CodeKey)
                    {

                        data.user = _mapper.Map<UserResponse>(user);
                        data.AuthToken = "";
                        data.RefreshToken = "";

                        response.StatusCode = 400;
                        response.Message = "El codigo no es valido";
                        response.Data = data;
                    }
                    else
                    {
                        data.user = _mapper.Map<UserResponse>(user);
                        data.AuthToken = "";
                        data.RefreshToken = "";

                        response.StatusCode = 200;
                        response.Message = "OK";
                        response.Data = data;
                    }
                } else
                {
                    data.user = _mapper.Map<UserResponse>(user);
                    data.AuthToken = "";
                    data.RefreshToken = "";

                    response.StatusCode = 400;
                    response.Message = "El codigo ya expiro, genera un nuevo codigo";
                    response.Data = data;
                }
               

                return response;
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
                var claims = new ClaimsIdentity();
                claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                var currentlyHour = DateTime.UtcNow;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    NotBefore = currentlyHour,
                    Expires = currentlyHour.AddMinutes(30),
                    SigningCredentials = credentials

                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenConfig = tokenHandler.CreateToken(token);

                return tokenHandler.WriteToken(tokenConfig);
            }
            catch (ArgumentNullException ex)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
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
                throw;
            }
            catch (EncoderFallbackException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
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
            var currentlyHour = DateTime.UtcNow.AddHours(-5);

            bool result;
            if (refreshTokenFinded.Count == 0)
            {
                var refreshTokenHistory = new RefreshTokenHistory
                {
                    UserId = user.Id,
                    Token = token,
                    RefreshToken = refreshToken,
                    CreationDate = currentlyHour,
                    ExpirationDate = currentlyHour.AddMinutes(30)
                };

                result = await _refreshTokenHistoryRepository.CreateAsync(refreshTokenHistory);
            }
            else
            {
                var userRefreshToken = refreshTokenFinded[0];

                userRefreshToken.Token = token;
                userRefreshToken.RefreshToken = refreshToken;
                userRefreshToken.CreationDate = currentlyHour;
                userRefreshToken.ExpirationDate = currentlyHour.AddMinutes(30);

                result = await _refreshTokenHistoryRepository.UpdateAsync(userRefreshToken);
            }
            return result;
        }

        public static string GenerateSecureToken(int length = 3)
        {
            byte[] tokenBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }

            return Convert.ToHexString(tokenBytes);
        }

        public async Task<bool> SendEmailRegister(Users user, string tokenConfirmation)
        {
            try
            {
                var htmlPath = Path.Combine(AppContext.BaseDirectory, "Email", "Templates", "ValidationEmailTemplate.html");
                if (!File.Exists(htmlPath))
                {
                    throw new FileNotFoundException($"No se encontró la plantilla HTML en la ruta: {htmlPath}");
                }

                var htmlTemplate = File.ReadAllText(htmlPath);
                var personalizedHtml = htmlTemplate.Replace("{{Token}}", tokenConfirmation);
                var email = new MimeMessage();

                var fromEmail = _config["SmtpConfig:Username"];

                email.From.Add(new MailboxAddress("In2sport", fromEmail));
                email.To.Add(MailboxAddress.Parse(user.Email));
                email.Subject = "CORREO CONFIRMACIÓN";
                var builder = new BodyBuilder
                {
                    HtmlBody = personalizedHtml
                };

                // Agregar imágenes embebidas (CID)
                var iconoPath = Path.Combine(AppContext.BaseDirectory, "Email", "Images", "icono.png");
                var imagen1Path = Path.Combine(AppContext.BaseDirectory, "Email", "Images", "imagen1.jpeg");
                var imagen2Path = Path.Combine(AppContext.BaseDirectory, "Email", "Images", "imagen2.jpeg");

                builder.LinkedResources.Add(iconoPath).ContentId = "LogoIn2Sports";
                builder.LinkedResources.Add(imagen1Path).ContentId = "ImagenPrincipal";
                builder.LinkedResources.Add(imagen2Path).ContentId = "ImagenSecundaria";

                email.Body = builder.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    try
                    {
                        await smtp.ConnectAsync(_config["SmtpConfig:Host"], 587, SecureSocketOptions.StartTls);
                        await smtp.AuthenticateAsync(fromEmail, _config["SmtpConfig:Password"]);
                        await smtp.SendAsync(email);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    finally
                    {
                        await smtp.DisconnectAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> SendEmailRecoverPassword(Users user, string tokenConfirmation)
        {
            try
            {
                string content = @"
                                    <!DOCTYPE html>
                                    <html lang='es'>
                                    <body>
                                        <div style='width:600px;padding:20px;border:1px solid #DBDBDB;border-radius:12px;font-family:Sans-serif'>
                                            <h1 style='color:#C76F61'>Recuperación de contraseña</h1>
                                            <p style='margin-bottom:25px'>Estimado/a&nbsp;<b>{0}</b>:</p>
                                            <p style='margin-bottom:25px'>
                                                Hemos recibido una solicitud para restablecer la contraseña de su cuenta. 
                                                Si no ha realizado esta solicitud, puede ignorar este mensaje.
                                            </p>
                                            <p style='margin-bottom:25px'>
                                                Para continuar con el restablecimiento de su contraseña, utilice la siguiente clave:
                                            </p>
                                            <p style='margin-bottom:25px'><b>{1}</b></p>
                                            <p style='margin-top:25px'>No respondas directamente a este correo electrónico generado automáticamente.</p>
                                            <p style='margin-top:25px'>Gracias.</p>
                                        </div>
                                    </body>
                                    </html>
                                    ";

                string htmlBody = string.Format(content, user.FirstName, tokenConfirmation);
                var email = new MimeMessage();

                var fromEmail = _config["SmtpConfig:Username"];

                email.From.Add(new MailboxAddress("In2sport", fromEmail));
                email.To.Add(MailboxAddress.Parse(user.Email));
                email.Subject = "RECUPERAR CONTRASEÑA";
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = htmlBody
                };

                using (var smtp = new SmtpClient())
                {
                    try
                    {
                        await smtp.ConnectAsync(_config["SmtpConfig:Host"], 587, SecureSocketOptions.StartTls);
                        await smtp.AuthenticateAsync(fromEmail, _config["SmtpConfig:Password"]); 
                        await smtp.SendAsync(email);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        await smtp.DisconnectAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private string? RemoveAllSpaces(object? input)
        {
            if (input == null) return null;

            var str = input.ToString();
            return string.IsNullOrWhiteSpace(str) ? null : Regex.Replace(str, @"[^a-zA-Z]", "");
        }

        public static string? KeepOnlyDigits(object? input)
        {
            if (input == null) return null;

            var str = input.ToString();
            if (string.IsNullOrWhiteSpace(str)) return null;
            return Regex.Replace(str, @"\D", "");
        }

        #endregion
    }
}
