using auth.in2sport.application.Services.UserServices.Response;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using auth.in2sport.infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using auth.in2sport.application.Services.UserServices.Request;
using MimeKit;
using MailKit.Security;
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.User;
using MercadoPago.Resource.Payment;
using System.Text.Json;

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
        private readonly IBaseRepository<UserSubscription> _userSubscriptionRepository;
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
        public UserService(IBaseRepository<Users> userRepository, IBaseRepository<UserType> userTypeRepository, IBaseRepository<UserSubscription> userSubscriptionRepository, IBaseRepository<AgeRange> ageRangeReposirity,IConfiguration config, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userTypeRepository = userTypeRepository ?? throw new ArgumentNullException(nameof(userTypeRepository));
            _userSubscriptionRepository = userSubscriptionRepository ?? throw new ArgumentNullException(nameof(userSubscriptionRepository));
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
                Console.WriteLine($"Error al obtener la lista de usuarios: {ex.Message}");
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
                        Console.WriteLine($"Error durante la activación del usuario: {ex.Message}");
                        throw new UpdateFailedException($"EError inesperado al inactivar el usuario: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de transacción: {ex.Message}");
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
                        Console.WriteLine($"Error durante la inactivación del usuario: {ex.Message}");
                        throw new UpdateFailedException($"EError inesperado al inactivar el usuario: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de transacción: {ex.Message}");
                throw new UpdateFailedException($"Error inesperado durante la transacción: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<UserResponse>> UpdateUser(UpdateUserRequest request)
        {
            using (var transaction = await _userRepository.BeginTransactionAsync())
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(request.Id);

                    if (user == null)
                    {
                        throw new UpdateFailedException("El usuario no existe", 400);
                    }

                    _mapper.Map(request, user);
                    UpdateChangedProperties(user, request);

                    var result = await _userRepository.UpdateAsync(user);

                    if (result)
                    {
                        await transaction.CommitAsync();
                        var response = new BaseResponse<UserResponse>
                        {
                            StatusCode = 200,
                            Message = "OK"
                        };
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

        public async Task<BaseResponse<List<UserResponse>>> GetByFilterAsync(string filter, Guid userId)
        {
            var response = new BaseResponse<List<UserResponse>>();

            try
            {
                var users = await _userRepository.GetByTwoFilterAsync
                    (entity => entity.FirstName == filter, entity => entity.Id != userId);
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
                Console.WriteLine($"Error al obtener la lista de usuarios: {ex.Message}");
                throw new FailedException($"Error inesperado al obtener la lista de usuarios: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<List<DataRegisteredeUsersResponse>>> GetDataRegisteredUsers(DateTime dateOne, DateTime dateTwo)
        {
            var response = new BaseResponse<List<DataRegisteredeUsersResponse>>();

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
                    (date, userGroup) => new DataRegisteredeUsersResponse
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
                Console.WriteLine($"Error al obtener datos: {ex.Message}");
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
                Console.WriteLine($"Error al obtener datos: {ex.Message}");
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
                Console.WriteLine($"Error al obtener datos: {ex.Message}");
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
                Console.WriteLine($"Error al obtener datos: {ex.Message}");
                throw new FailedException($"Error inesperado al obtener datos: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<UserResponse>> Ticket(CreateTicketRequest request)
        {
            var response = new BaseResponse<UserResponse>();

            try
            {
                var user = await _userRepository.GetByIdAsync(request.UserId);

                var result = await SendEmail(request, user);
                if (!result)
                {
                    throw new UpdateFailedException("Error al enviar correo", 400);
                }


                response.StatusCode = 200;
                response.Message = "OK";
                return response;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer ticket: {ex.Message}");
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
                Console.WriteLine($"Error al obtener validacion: {ex.Message}");
                throw new FailedException($"Error inesperado al obtener validacion: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<dynamic>> CreatePreference(PreferenceDtoRequest request)
        {
            var response = new BaseResponse<dynamic>();

            try
            {
                MercadoPagoConfig.AccessToken = _config["MercadoPago:AccessToken"];
                var preferenceRequest = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = request.Title,
                            Quantity = request.Quantity,
                            UnitPrice = request.Price
                        }
                    },
                    BackUrls = new PreferenceBackUrlsRequest
                    {
                        Success = _config["MercadoPago:BackUrl"],
                        Failure = _config["MercadoPago:BackUrl"],
                        Pending = _config["MercadoPago:BackUrl"]
                    },
                    
                    Metadata = new Dictionary<string, object>
                    {
                        { "user_id", request.UserId.ToString() },
                        { "course_id", request.CourseId.ToString() },

                    },
                    AutoReturn = "approved",
                    NotificationUrl = _config["MercadoPago:NotificationUrl"]
                };

                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(preferenceRequest);

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = new
                {
                    id = preference.Id,
                    init_point = preference.InitPoint,
                    sandbox_init_point = preference.SandboxInitPoint
                };
                return response;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer pago: {ex.Message}");
                throw new FailedException($"Error inesperado al hacer pago: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<dynamic>> CreatePreferenceLeague(PreferenceLeagueDtoRequest request)
        {
            var response = new BaseResponse<dynamic>();

            try
            {
                MercadoPagoConfig.AccessToken = _config["MercadoPago:AccessToken"];
                var preferenceRequest = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = request.Title,
                            Quantity = request.Quantity,
                            UnitPrice = request.Price
                        }
                    },
                    BackUrls = new PreferenceBackUrlsRequest
                    {
                        Success = _config["MercadoPago:BackUrl"],
                        Failure = _config["MercadoPago:BackUrl"],
                        Pending = _config["MercadoPago:BackUrl"]
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        { "league_users", JsonSerializer.Serialize(request.Data) },
                    },
                    AutoReturn = "approved",
                    NotificationUrl = _config["MercadoPago:NotificationUrlLeague"]
                };

                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(preferenceRequest);

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = new
                {
                    id = preference.Id,
                    init_point = preference.InitPoint,
                    sandbox_init_point = preference.SandboxInitPoint
                };
                return response;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer pago: {ex.Message}");
                throw new FailedException($"Error inesperado al hacer pago: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<dynamic>> NotificationsMercadopago(NotificationRequest request)
        {
            var response = new BaseResponse<dynamic>();

            try
            {
                MercadoPagoConfig.AccessToken = _config["MercadoPago:AccessToken"];

                if (request.Type == "payment" && (request.Action == "payment.created" || request.Action == "payment.updated"))
                {
                    var client = new PaymentClient();
                    var payment = await client.GetAsync(long.Parse(request.Data.Id));

                    if (payment.Status == "approved")
                    {
                        Guid userId = Guid.Parse(payment.Metadata["user_id"].ToString());
                        Guid courseId = Guid.Parse(payment.Metadata["course_id"].ToString());

                        var validationSubsctiption = await _userSubscriptionRepository.GetByTwoFilterAsync
                                    (entity => entity.UserId == userId, entity => entity.CourseId == courseId);

                        DateTime utcNow = DateTime.UtcNow;
                        DateTime localDate = utcNow.AddHours(-5).Date;

                        if (validationSubsctiption.Count > 0)
                        {
                            UserSubscription subscription = validationSubsctiption[0];

                            subscription.MonthsSubscribed = subscription.MonthsSubscribed + 1;
                            subscription.LastDate = utcNow;

                            var result = await _userSubscriptionRepository.UpdateAsync(subscription);
                        }
                        else
                        {
                            UserSubscription subscription = new UserSubscription
                            {
                                UserId = userId,
                                CourseId = courseId,
                                MonthsSubscribed = 1,
                                LastDate = utcNow
                            };
                            var result = await _userSubscriptionRepository.CreateAsync(subscription);
                        }
                           
                    }
                }

                response.StatusCode = 200;
                response.Message = "OK";
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer ticket: {ex.Message}");
                throw new FailedException($"Error inesperado al hacer ticket: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<dynamic>> NotificationsMercadopagoLeague(NotificationRequest request)
        {
            var response = new BaseResponse<dynamic>();

            try
            {
                MercadoPagoConfig.AccessToken = _config["MercadoPago:AccessToken"];

                if (request.Type == "payment" && (request.Action == "payment.created" || request.Action == "payment.updated"))
                {
                    var client = new PaymentClient();
                    var payment = await client.GetAsync(long.Parse(request.Data.Id));

                    if (payment.Status == "approved")
                    {
                        List<LeagueUser> leagueUsers = JsonSerializer.Deserialize<List<LeagueUser>>(payment.Metadata["league_users"].ToString());
                        foreach (var item in leagueUsers!)
                        {
                            foreach (var course in item.Courses!)
                            {
                                var validationSubsctiption = await _userSubscriptionRepository.GetByTwoFilterAsync
                                   (entity => entity.UserId == item.User!.Id, entity => entity.CourseId == Guid.Parse(course));

                                DateTime utcNow = DateTime.UtcNow;
                                DateTime localDate = utcNow.AddHours(+5).Date;

                                if (validationSubsctiption.Count > 0)
                                {
                                    UserSubscription subscription = validationSubsctiption[0];

                                    subscription.MonthsSubscribed = subscription.MonthsSubscribed + 1;
                                    subscription.LastDate = localDate;

                                    var result = await _userSubscriptionRepository.UpdateAsync(subscription);
                                }
                                else
                                {
                                    UserSubscription subscription = new UserSubscription
                                    {
                                        UserId = item.User!.Id,
                                        CourseId = Guid.Parse(course),
                                        MonthsSubscribed = 1,
                                        LastDate = localDate
                                    };
                                    var result = await _userSubscriptionRepository.CreateAsync(subscription);
                                }
                            }    
                        }
                    }
                }

                response.StatusCode = 200;
                response.Message = "OK";
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer ticket: {ex.Message}");
                throw new FailedException($"Error inesperado al hacer ticket: {ex.Message}", 500);
            }
        }

        #region Private Methods

        private void UpdateChangedProperties(Users originalUser, UpdateUserRequest request)
        {
            var propertyInfos = typeof(UpdateUserRequest).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                var originalValue = originalUser.GetType().GetProperty(propertyName)?.GetValue(originalUser, null);
                var requestValue = propertyInfo.GetValue(request, null);

                if (originalValue != null && !originalValue.Equals(requestValue))
                {
                    originalUser.GetType().GetProperty(propertyName)?.SetValue(originalUser, requestValue);
                }
            }
        }

        public async Task<bool> SendEmail(CreateTicketRequest request, Users user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("In2sport", "gonsalez.carlos@live.com.mx"));
            message.To.Add(new MailboxAddress("Carlos", "krlsoh1096@gmail.com"));
            //message.To.Add(new MailboxAddress("Luis", "jemab2@hotmail.com"));

            message.Subject = request.Tittle;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
            <html>
            <body>
                <p>Nombre de usuario: {user.FirstName} {user.SecondName} {user.FirstLastname} {user.SecondLastname}</p> 
                <p>Email: {user.Email}</p>
                <p>Desripción de ticket: </p>
                <p>{request.Description}</p>
            </body>
            </html>";

            message.Body = bodyBuilder.ToMessageBody();
            //message.Body = new TextPart("plain")
            //{
            //    Text = request.Description
            //};
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                    // Autenticar con el servidor SMTP
                    await client.AuthenticateAsync("gonsalez.carlos@live.com.mx", "Jashuarr0103");

                    // Enviar el correo
                    await client.SendAsync(message);

                    // Desconectar del servidor SMTP
                    await client.DisconnectAsync(true);

                    // Retornar true si el envío es exitoso
                    return true;
                }
                catch (Exception ex)
                {
                    // Opcional: Registrar el error
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Retornar false si ocurre un error
                    return false;
                }
                finally
                {
                    // Asegurarse de liberar los recursos del cliente SMTP
                    client.Dispose();
                }
            }
        }

        #endregion

    }
}
