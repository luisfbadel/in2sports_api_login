using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.UserServices;
using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace auth.in2sport.api.Controllers
{
    public class UserController : Controller
    {
        #region Private Properties

        /// <summary>
        /// Instance of the User Service
        /// </summary>
        private readonly IUserService _userService;

        #endregion

        #region Constructor

        /// <summary>
        /// Defines constructor
        /// </summary>
        /// <param name="userService"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        #endregion


        [Route("api/v1/user/get-all")]
        [HttpGet]
        public async Task<IActionResult> getAll(int page = 1, int pageSize = 10)
        {
           var result = await _userService.GetUsers(page, pageSize);
           return Ok(result);
        }

        [Route("api/v1/user/activate-user")]
        [HttpPost]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.ActivateUser(id));
            }
            return BadRequest();
        }

        [Route("api/v1/user/inactivate-user")]
        [HttpPost]
        public async Task<IActionResult> InactivateUser(Guid id)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.InactivateUser(id));
            }
            return BadRequest();
        }
    }
}
