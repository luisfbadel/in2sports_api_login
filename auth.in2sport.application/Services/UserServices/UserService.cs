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
using System.Text.Json;
using MercadoPago.Client.Preapproval;
using RestSharp;
using MimeKit.Text;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

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
        private readonly IBaseRepository<PaymentRecordInstitution> _paymentRecordInstitution;
        private readonly IBaseRepository<SubscriptionVerification> _subscriptionVerification;
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
        public UserService(IBaseRepository<Users> userRepository, IBaseRepository<UserType> userTypeRepository, IBaseRepository<UserSubscription> userSubscriptionRepository, IBaseRepository<AgeRange> ageRangeReposirity, IBaseRepository<PaymentRecordInstitution> paymentRecordInstitution, IBaseRepository<SubscriptionVerification> subscriptionVerification, IConfiguration config, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userTypeRepository = userTypeRepository ?? throw new ArgumentNullException(nameof(userTypeRepository));
            _userSubscriptionRepository = userSubscriptionRepository ?? throw new ArgumentNullException(nameof(userSubscriptionRepository));
            _ageRangeRepository = ageRangeReposirity ?? throw new ArgumentNullException(nameof(ageRangeReposirity));
            _paymentRecordInstitution = paymentRecordInstitution ?? throw new ArgumentNullException(nameof(paymentRecordInstitution));
            _subscriptionVerification = subscriptionVerification ?? throw new ArgumentNullException(nameof(subscriptionVerification));
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

                    user.FirstName = request.FirstName;
                    user.SecondName = request.SecondName;
                    user.FirstLastname = request.FirstLastname;
                    user.SecondLastname = request.SecondLastname;
                    user.DocumentNumber = request.DocumentNumber;
                    user.PhoneNumber = request.PhoneNumber;
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
                //var users = await _userRepository.GetByTwoFilterAsync(entity => entity.FirstName == request.Filter, entity => entity.Id != request.UserId);

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
                Console.WriteLine($"Error al obtener la lista de usuarios: {ex.Message}");
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

                string content = @"
                                    <!DOCTYPE html>
                                    <html lang='es'>
                                    <body>
                                        <div style='width:600px;padding:20px;border:1px solid #DBDBDB;border-radius:12px;font-family:Sans-serif'>
                                            <h1 style='color:#C76F61'>Ticket</h1>
                                            <p style='margin-bottom:25px'>{0}</p>
                                            <p style='margin-bottom:25px'>{1}</p>
                                            <p style='margin-bottom:25px'>{2}</p>
                                        </div>
                                    </body>
                                    </html>";
                string userName = user.FirstName + " " + user.SecondName + " " + user.FirstLastname + " " + user.SecondLastname; 
                string htmlBody = string.Format(content, request.Description, userName, user.Email);
                var options = new RestClientOptions("https://8k5kz9.api.infobip.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var requestEmail = new RestRequest("/email/3/send", Method.Post);
                requestEmail.AddHeader("Authorization", "App ed7310401857e5fb447f3794aa6f6020-e57679ab-8886-49d1-b0cf-f046c2ae92b3");
                requestEmail.AddHeader("Content-Type", "multipart/form-data");
                requestEmail.AddHeader("Accept", "application/json");
                requestEmail.AlwaysMultipartFormData = true;
                requestEmail.AddParameter("from", "Carlos <gonsalez.carlos@live.com.mx>");
                requestEmail.AddParameter("subject", request.Tittle);
                requestEmail.AddParameter("to", "{\"to\":\"krlosh1096@gmail.com\",\"placeholders\":{\"firstName\":\"Carlos\"}}");
                requestEmail.AddParameter("html", htmlBody);
                RestResponse result = await client.ExecuteAsync(requestEmail);

                //var result = await SendEmail(request, user);
                //if (!result)
                //{
                //    throw new UpdateFailedException("Error al enviar correo", 400);
                //}


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

                        DateTime localDate = DateTime.UtcNow;

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
                                UserId = userId,
                                CourseId = courseId,
                                MonthsSubscribed = 1,
                                LastDate = localDate
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

                                DateTime localDate = DateTime.UtcNow;

                                if (validationSubsctiption.Count > 0)
                                {
                                    UserSubscription subscription = validationSubsctiption[0];

                                    subscription.MonthsSubscribed = subscription.MonthsSubscribed + 1;
                                    subscription.LastDate = localDate;

                                    var result = await _userSubscriptionRepository.UpdateAsync(subscription);

                                    if (result)
                                    {
                                        PaymentRecordInstitution paymentInstitution = new PaymentRecordInstitution
                                        {
                                            StudentId = item.User!.Id,
                                            CourseId = Guid.Parse(course),
                                            InstitutionId = item.User!.Id,
                                            StartDate = localDate,
                                            EndDate = localDate.AddDays(30)
                                        };

                                        var ressultInstitution = await _paymentRecordInstitution.CreateAsync(paymentInstitution);
                                    }
                                    
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

                                    if (result)
                                    {
                                        PaymentRecordInstitution paymentInstitution = new PaymentRecordInstitution
                                        {
                                            StudentId = item.User!.Id,
                                            CourseId = Guid.Parse(course),
                                            InstitutionId = item.User!.Id,
                                            StartDate = localDate,
                                            EndDate = localDate.AddDays(30)
                                        };

                                        var ressultInstitution = await _paymentRecordInstitution.CreateAsync(paymentInstitution);
                                    }
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

        public async Task<BaseResponse<dynamic>> CreateSuscription(PreapprovalRequest request)
        {
            var response = new BaseResponse<dynamic>();

            try
            {
                //MercadoPagoConfig.AccessToken = _config["MercadoPago:AccessToken"];
                MercadoPagoConfig.AccessToken = "APP_USR-8688261876484762-071113-727aa9906c5322115be7f5176ca51ddf-437698549";
                var client = new PreapprovalClient();

                var preapprovalRequest = new PreapprovalCreateRequest
                {
                    PayerEmail = request.PayerEmail,
                    Reason = request.Reason,
                    AutoRecurring = new PreApprovalAutoRecurringCreateRequest
                    {
                        Frequency = request.Frequency,
                        FrequencyType = request.FrequencyType,
                        TransactionAmount = request.AutoRecurringAmount,
                        CurrencyId = request.CurrencyId,
                    },
                    BackUrl = "https://www.youtube.com/",
                };

                var preapproval = await client.CreateAsync(preapprovalRequest);

                var subscriptionVerification = new SubscriptionVerification
                {
                    SubscriptionId = preapproval.Id,
                    UserId = request.UserId,
                    CourseId = request.CourseId,
                };

                var verificationCreation = await _subscriptionVerification.CreateAsync(subscriptionVerification);

                response.StatusCode = 200;
                response.Message = "OK";
                response.Data = new
                {
                    id = preapproval.Id,
                    init_point = preapproval.InitPoint,
                    sandbox_init_point = preapproval.SandboxInitPoint
                };

                return response;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer pago: {ex.Message}");
                throw new FailedException($"Error inesperado al crear suscripcion: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<dynamic>> NotificationsSuscription(NotificationRequest request)
        {
            var response = new BaseResponse<dynamic>();

            try
            {
                MercadoPagoConfig.AccessToken = "APP_USR-8688261876484762-071113-727aa9906c5322115be7f5176ca51ddf-437698549";

                if (request.Type == "subscription_authorized_payment" && request.Action == "created")
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "APP_USR-8688261876484762-071113-727aa9906c5322115be7f5176ca51ddf-437698549");
                    //var subscripcionResponse = await client.GetAsync($"https://api.mercadopago.com/preapproval/{request.Data.Id}");
                    var subscripcionPayment = await client.GetAsync($"https://api.mercadopago.com/authorized_payments/{request.Data.Id}");

                    //var content = await subscripcionResponse.Content.ReadAsStringAsync();
                    var contentPayment = await subscripcionPayment.Content.ReadAsStringAsync();

                    var suscripcion = JObject.Parse(contentPayment);
                    //var suscripcionPayment = JObject.Parse(contentPayment);


                    //if (suscripcion["status"]?.ToString() == "authorized")
                    //{
                        //var subscriptioninfo = await _subscriptionVerification.GetByFilterAsync(entity => entity.SubscriptionId == request.Data.Id);
                        var subscriptioninfo = await _subscriptionVerification.GetByFilterAsync(entity => entity.SubscriptionId == suscripcion["preapproval_id"].ToString());

                        Guid userid = subscriptioninfo[0].UserId;
                        Guid courseid = subscriptioninfo[0].CourseId;

                        var validationsubsctiption = await _userSubscriptionRepository.GetByTwoFilterAsync(entity => entity.UserId == userid, entity => entity.CourseId == courseid);

                        DateTime localdate = DateTime.UtcNow;

                        if (validationsubsctiption.Count > 0)
                        {
                            UserSubscription subscription = validationsubsctiption[0];

                            subscription.MonthsSubscribed = subscription.MonthsSubscribed + 1;
                            subscription.LastDate = localdate;

                            var result = await _userSubscriptionRepository.UpdateAsync(subscription);
                        }
                        else
                        {
                            UserSubscription subscription = new UserSubscription
                            {
                                UserId = userid,
                                CourseId = courseid,
                                MonthsSubscribed = 1,
                                LastDate = localdate
                            };
                            var result = await _userSubscriptionRepository.CreateAsync(subscription);
                        }

                    //}
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

                email.From.Add(new MailboxAddress("In2sport", "krlosh1096@gmail.com"));
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
                        // Conectar al servidor SMTP
                        await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                        // Autenticar con las credenciales
                        await smtp.AuthenticateAsync("krlosh1096@gmail.com", "hnysgpmhdbnouoyh"); // Reemplaza con tu correo y clave de aplicación

                        // Enviar el correo
                        await smtp.SendAsync(email);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Maneja las excepciones aquí (logs, reintentos, etc.)
                        throw;
                    }
                    finally
                    {
                        // Desconectar del servidor SMTP
                        await smtp.DisconnectAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Opcional: Registrar el error
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Retornar false si ocurre un error
                return false;
            }
            
        }

        #endregion

    }
}
