using auth.in2sport.application.Services.UserServices;
using auth.in2sport.application.Services.UserServices.Request;
using Microsoft.AspNetCore.Mvc;

namespace auth.in2sport.api.Controllers
{
    [ApiController]
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
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 30)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetUsers(page, pageSize));
            }
            return BadRequest();
        }

        [Route("api/v1/user/update-user")]
        [HttpPatch]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.UpdateUser(request));
            }
            return BadRequest();

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

        [Route("api/v1/user/get-by-filter")]
        [HttpGet]
        public async Task<IActionResult> GetByFilterAsync(string filter)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetByFilterAsync(filter));
            }
            return BadRequest();
        }
    }
}
