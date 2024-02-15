using auth.in2sport.application.Services.UserServices.Request;
using auth.in2sport.application.Services.UserServices.Response;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using auth.in2sport.infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using System.Linq.Expressions;

namespace auth.in2sport.application.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IBaseRepository<Users> _userRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public UserService(IBaseRepository<Users> userRepository, IConfiguration config, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _config = config ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }
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
            user.status = 1;
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
            user.status = 0;
            var response = new BaseResponse<UserResponse>();
            var result = await _userRepository.UpdateAsync(user);
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
    }
}
