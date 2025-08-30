using auth.in2sport.application.Services.UserServices.Response;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using auth.in2sport.infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using auth.in2sport.application.Services.UserServices.Request;
using MimeKit;
using MailKit.Security;
using RestSharp;
using MimeKit.Text;
using MailKit.Net.Smtp;
using System.Text.RegularExpressions;

namespace auth.in2sport.application.Services.UserServices
{
    public class UserService : IUserService
    {

        #region Pirvate Properties

        /// <summary>
        /// Instance of the Base Repository
        /// Instance of the Base Configuration
        /// Instance of the Base Mapper
        /// </summary>
        private readonly IBaseRepository<Users> _userRepository;
        private readonly IBaseRepository<UserType> _userTypeRepository;
        private readonly IBaseRepository<AgeRange> _ageRangeRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor

        /// <summary>
        /// Defines constructor
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="config"></param>
        /// <param name="mapper"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserService(IBaseRepository<Users> userRepository, IBaseRepository<UserType> userTypeRepository, IBaseRepository<AgeRange> ageRangeReposirity, IConfiguration config, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userTypeRepository = userTypeRepository ?? throw new ArgumentNullException(nameof(userTypeRepository));
            _ageRangeRepository = ageRangeReposirity ?? throw new ArgumentNullException(nameof(ageRangeReposirity));
            _config = config ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        #endregion

        public async Task<BaseResponse<List<UserResponse>>> GetUsers(int page, int pageSize)
        {
            var response = new BaseResponse<List<UserResponse>>();

            try
            {
                var users = await _userRepository.GetAsync();
                var listUsers = users
                    .Select(o => _mapper.Map<UserResponse>(o))
                    .ToList();
                var totalCount = listUsers.Count;
                var totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
                var usersPerPage = listUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = usersPerPage;
                return response;
            }
            catch (Exception ex)
            {
                throw new UserFailedException($"Error inesperado al obtener la lista de usuarios: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<UserResponse>> ActivateUser(Guid id)
        {
            var response = new BaseResponse<UserResponse>();

            try
            {
                using (var transaction = await _userRepository.BeginTransactionAsync())
                {
                    try
                    {
                        var user = await _userRepository.GetByIdAsync(id);
                        user.Status = 1;

                        var result = await _userRepository.UpdateAsync(user);

                        transaction.Commit();

                        if (!result)
                        {
                            throw new UpdateFailedException("Ocurrió un error al activar el usuario.", 400);
                        }

                        response.StatusCode = 200;
                        response.Message = "OK";
                        return response;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new UpdateFailedException($"EError inesperado al inactivar el usuario: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UpdateFailedException($"Error inesperado durante la transacción: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<UserResponse>> InactivateUser(Guid id)
        {
            var response = new BaseResponse<UserResponse>();

            try
            {
                using (var transaction = await _userRepository.BeginTransactionAsync())
                {
                    try
                    {
                        var user = await _userRepository.GetByIdAsync(id);
                        user.Status = 0;

                        var result = await _userRepository.UpdateAsync(user);

                        transaction.Commit();

                        if (!result)
                        {
                            throw new UpdateFailedException("Ocurrió un error al inactivar el usuario.", 400);
                        }

                        response.StatusCode = 200;
                        response.Message = "OK";

                        return response;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new UpdateFailedException($"EError inesperado al inactivar el usuario: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UpdateFailedException($"Error inesperado durante la transacción: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<UserResponse>> UpdateUser(UpdateUserRequest request)
        {
            var response = new BaseResponse<UserResponse>();
 

            using (var transaction = await _userRepository.BeginTransactionAsync())
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(request.Id);

                    if (user == null)
                    {
                        throw new UpdateFailedException("El usuario no existe", 400);
                    }

                    var resultDocumentNumber = await _userRepository.GetByTwoFilterAsync(entity => entity.DocumentNumber == request.DocumentNumber, entity => entity.Id != request.Id);

                    if (resultDocumentNumber.Count > 0)
                    {
                        response.StatusCode = 400;
                        response.Message = "La cedula ya existe";

                        return response;
                    }

                    user.FirstName = RemoveAllSpaces(request.FirstName);
                    user.SecondName = RemoveAllSpaces(request.SecondName);
                    user.FirstLastname = RemoveAllSpaces(request.FirstLastname);
                    user.SecondLastname = RemoveAllSpaces(request.SecondLastname);
                    user.DocumentNumber = long.Parse(KeepOnlyDigits(request.DocumentNumber));
                    user.PhoneNumber = long.Parse(KeepOnlyDigits(request.PhoneNumber));
                    user.Address = request.Address;
                    user.CreationDate = user.CreationDate.ToUniversalTime();
                    user.Birthdate = request.Birthdate.ToUniversalTime();
                    user.InstitutionName = request.InstitutionName;
                    user.Departament = request.Departament;
                    user.City = request.City;

                    var result = await _userRepository.UpdateAsync(user);

                    if (result)
                    {
                        await transaction.CommitAsync();

                        response.StatusCode = 200;
                        response.Message = "OK";
                        response.Data = _mapper.Map<UserResponse>(user);

                        return response;
                    }
                    else
                    {
                        throw new UpdateFailedException("La actualización del usuario falló.", 400);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new UpdateFailedException($"Error al actualizar: {ex.Message}", 500);
                }
            }
        }

        public async Task<BaseResponse<List<UserResponse>>> GetByFilterAsync(UsersFiltersRequest request)
        {
            var response = new BaseResponse<List<UserResponse>>();

            try
            {
                bool hasLongFieldFilter = long.TryParse(request.Filter, out long longFieldValue);

                var users = await _userRepository.GetByTwoFilterAsync(
                                    entity => (entity.FirstName.ToLower() == request.Filter.ToLower() ||
                                              entity.SecondName.ToLower() == request.Filter.ToLower() ||
                                              entity.FirstLastname.ToLower() == request.Filter.ToLower() ||
                                              entity.SecondLastname.ToLower() == request.Filter.ToLower() ||
                                              entity.DocumentNumber == longFieldValue), entity => entity.Id != request.UserId);
                var listUsers = users
                    .Select(o => _mapper.Map<UserResponse>(o))
                    .ToList();

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = listUsers;
                return response;
            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al obtener la lista de usuarios: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<List<DataRegisteredUsersResponse>>> GetDataRegisteredUsers(DateTime dateOne, DateTime dateTwo)
        {
            var response = new BaseResponse<List<DataRegisteredUsersResponse>>();

            try
            {

                var usersForMonth = await _userRepository.GetDataForMonthAndYearAsync
                    (entity => entity.CreationDate >= dateOne, entity => entity.CreationDate <= dateTwo);

                var allDates = Enumerable.Range(0, (dateTwo.Year - dateOne.Year) * 12 + dateTwo.Month - dateOne.Month + 1)
                  .Select(m => new {
                      Year = dateOne.AddMonths(m).Year,
                      Month = dateOne.AddMonths(m).Month
                  }).ToList();

                var groupedData = allDates.GroupJoin(
                    usersForMonth,
                    date => new { date.Year, date.Month },
                    user => new { user.CreationDate!.Year, user.CreationDate!.Month },
                    (date, userGroup) => new DataRegisteredUsersResponse
                    {
                        Month = date.Month,
                        Year = date.Year,
                        UsersRegister = userGroup.Count()
                    })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = groupedData;
                return response;

            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al obtener datos: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<DataUsersStatusResponse>> GetUsersStatus()
        {
            var response = new BaseResponse<DataUsersStatusResponse>();

            try
            {
                var userAll = await _userRepository.GetAsync();

                var usersForStatus = await _userRepository.GetByFilterAsync(entity => entity.Status == 1);

                var data = new DataUsersStatusResponse
                {
                    UsersActives = usersForStatus.Count(),
                    UsersInactives = userAll.Count - usersForStatus.Count()
                };

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = data;
                return response;

            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al obtener datos: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<List<UserType>>> GetUseTypes()
        {
            var response = new BaseResponse<List<UserType>>();

            try
            {
                var typesUser = await _userTypeRepository.GetAsync();

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = typesUser;
                return response;

            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al obtener datos: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<List<AgeRange>>> GetAgeRange()
        {
            var response = new BaseResponse<List<AgeRange>>();

            try
            {
                var agerange = await _ageRangeRepository.GetAsync();

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = agerange;
                return response;

            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al obtener datos: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<UserResponse>> Ticket(CreateTicketRequest request)
        {
            var response = new BaseResponse<UserResponse>();

            try
            {
                var user = await _userRepository.GetByIdAsync(request.UserId);

                // Construir HTML
                string content = @"
                    <!DOCTYPE html>
                    <html lang='es'>
                    <body>
                        <div style='width:600px;padding:20px;border:1px solid #DBDBDB;border-radius:12px;font-family:Sans-serif'>
                            <h1 style='color:#C76F61'>Nuevo Ticket</h1>
                            <p style='margin-bottom:25px'><b>Descripción:</b> {0}</p>
                            <p style='margin-bottom:25px'><b>Usuario:</b> {1}</p>
                            <p style='margin-bottom:25px'><b>Email:</b> {2}</p>
                        </div>
                    </body>
                    </html>";

                string userName = $"{user.FirstName} {user.SecondName} {user.FirstLastname} {user.SecondLastname}";
                string htmlBody = string.Format(content, request.Description, userName, user.Email);

                var email = new MimeMessage();
                var fromEmail = _config["SmtpConfig:Username"];

                email.From.Add(new MailboxAddress("In2sport", fromEmail));
                email.To.Add(MailboxAddress.Parse("in2sports08@gmail.com"));
                email.Subject = request.Tittle;

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
                    }
                    catch (Exception ex)
                    {
                        throw new FailedException($"Error al enviar ticket: {ex.Message}", 500);
                    }
                    finally
                    {
                        await smtp.DisconnectAsync(true);
                    }
                }

                response.StatusCode = 200;
                response.Message = "Ticket enviado correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al hacer ticket: {ex.Message}", 500);
            }
        }


        public async Task<BaseResponse<bool>> GetValidationUser(string email)
        {
            var response = new BaseResponse<bool>();

            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    response.StatusCode = 200;
                    response.Message = "OK";
                    response.Data = true;
                    return response;
                }
                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = false;
                return response;
            }
            catch (Exception ex)
            {
                throw new FailedException($"Error inesperado al obtener validacion: {ex.Message}", 500);
            }
        }

        #region Private Methods

        public async Task<bool> SendEmail(CreateTicketRequest request, Users user)
        {
            try
            {
                string content = @"
                                    <!DOCTYPE html>
                                    <html lang='es'>
                                    <body>
                                        <div style='width:600px;padding:20px;border:1px solid #DBDBDB;border-radius:12px;font-family:Sans-serif'>
                                            <h1 style='color:#C76F61'>Confirmar correo electrónico</h1>
                                            <p style='margin-bottom:25px'>Estimado/a&nbsp;<b>{0}</b>:</p>
                                            <p style='margin-bottom:25px'>Gracias por abrir una cuenta con nosotros. Para utilizar su cuenta, primero deberá confirmar su correo electrónico haciendo clic en el botón a continuación.</p>
                                            <a style='padding:12px;border-radius:12px;background-color:#6181C7;color:#fff;text-decoration:none' href='{1}' target='_blank'>Confirme su correo electrónico</a>
                                            <p style='margin-top:25px'>Gracias.</p>
                                        </div>
                                    </body>
                                    </html>";
                string url = "https://www.youtube.com/";
                string htmlBody = string.Format(content, user.FirstName, url);
                var email = new MimeMessage();
                var fromEmail = _config["SmtpConfig:Username"];
                email.From.Add(new MailboxAddress("In2sport", fromEmail));
                email.To.Add(MailboxAddress.Parse(user.Email));
                email.Subject = "Correo Confirmacion";
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
                throw new UserFailedException($"Error al enviar correo: {ex.Message}", 500);
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
