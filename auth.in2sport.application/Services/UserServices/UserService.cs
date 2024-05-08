using auth.in2sport.application.Services.UserServices.Response;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using auth.in2sport.infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using auth.in2sport.application.Services.UserServices.Request;

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
        private readonly IBaseRepository<TypeUser> _typeUserRepository;
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
        public UserService(IBaseRepository<Users> userRepository, IBaseRepository<TypeUser> typeUserRepository, IConfiguration config, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _typeUserRepository = typeUserRepository ?? throw new ArgumentNullException(nameof(typeUserRepository));
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

        public async Task<BaseResponse<List<TypeUser>>> GetTypesUser()
        {
            var response = new BaseResponse<List<TypeUser>>();

            try
            {
                var typesUser = await _typeUserRepository.GetAsync();

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

        #endregion

    }
}
