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
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        #endregion

        #region

        /// <summary>
        /// Defines constructor
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="config"></param>
        /// <param name="mapper"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserService(IBaseRepository<Users> userRepository, IConfiguration config, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _config = config ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        #endregion
        public async Task<BaseResponse<List<UserResponse>>> GetUsers(int page, int pageSize)
        { 
            var users = await _userRepository.GetAsync();
            var listUsers = users
                .Select(o => _mapper.Map<UserResponse>(o))
                .ToList();
            var totalCount = listUsers.Count;
            var totalPages = (int)Math.Ceiling((decimal)totalCount/pageSize);
            var usersPerPage = listUsers.Skip((page - 1) * totalPages).Take(pageSize).ToList();

            var response = new BaseResponse<List<UserResponse>>();
            response.StatusCode = 200;
            response.Message = "OK";
            response.Data = usersPerPage;
            return response;
        }

        public async Task<BaseResponse<UserResponse>> ActivateUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            user.Status = 1;
            var result = await _userRepository.UpdateAsync(user);
            var response = new BaseResponse<UserResponse>();
            if (result)
            {
                response.StatusCode = 200;
                response.Message = "OK";
            }
            else
            {
                response.StatusCode = 400;
                response.Message = "Error al Activar";
            }
            return response;
        }
        public async Task<BaseResponse<UserResponse>> InactivateUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            user.Status = 0;
            var result = await _userRepository.UpdateAsync(user);
            var response = new BaseResponse<UserResponse>();
            if (result)
            {
                response.StatusCode = 200;
                response.Message = "OK";
            }
            else
            {
                response.StatusCode = 400;
                response.Message = "Error al Inactivar";
            }
            return response;
        }
        public async Task<BaseResponse<UserResponse>> UpdateUser(UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            _mapper.Map(request, user);
            UpdateChangedProperties(user, request);
            var result = await _userRepository.UpdateAsync(user);
            var response = new BaseResponse<UserResponse>();
            if (result)
            {
                response.StatusCode = 200;
                response.Message = "OK";
            }
            else
            {
                response.StatusCode = 400;
                response.Message = "Error al actualizar";
            }
            return response;
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
