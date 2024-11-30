using auth.in2sport.application.Services.UserServices;
using auth.in2sport.application.Services.UserServices.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

        //[Authorize]
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

        //[Authorize]
        [Route("api/v1/user/update-user")]
        [HttpPatch]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.UpdateUser(request));
            }
            return BadRequest();

        }

        //[Authorize]
        [Route("api/v1/user/activate-user")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.ActivateUser(id));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/inactivate-user")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> InactivateUser(Guid id)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.InactivateUser(id));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/get-by-filter")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetByFilterAsync(UsersFiltersRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetByFilterAsync(request));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/get-registered-users")]
        [HttpGet]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetData(DateTime dateOne, DateTime dateTwo)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetDataRegisteredUsers(dateOne.ToUniversalTime(), dateTwo.ToUniversalTime()));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/get-users-status")]
        [HttpGet]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetUsersStatus()
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetUsersStatus());
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/get-user-types")]
        [HttpGet]
        public async Task<IActionResult> GetUseTypes()
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetUseTypes());
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/get-age-range")]
        [HttpGet]
        public async Task<IActionResult> GetAgeRange()
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetAgeRange());
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/ticket")]
        [HttpPost]
        public async Task<IActionResult> Ticket(CreateTicketRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.Ticket(request));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/get-validation-user")]
        [HttpGet]
        public async Task<IActionResult> GetValidationUser(string email)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.GetValidationUser(email));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/create_preference")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> CreatePreference(PreferenceDtoRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.CreatePreference(request));
            }
            return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/user/create_preference_league")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> CreatePreferenceLeague(PreferenceLeagueDtoRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.CreatePreferenceLeague(request));
            }
            return BadRequest();
        }

        [Route("api/v1/user/notifications_mercadopago")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> NotificationsMercadopago(NotificationRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.NotificationsMercadopago(request));
            }
            return BadRequest();
        }

        [Route("api/v1/user/notifications_mercadopago_league")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> NotificationsMercadopagoLeague(NotificationRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.NotificationsMercadopagoLeague(request));
            }
            return BadRequest();
        }

        [Route("api/v1/user/create_suscription")]
        [HttpPost]
        //[EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> CreateSuscription(PreapprovalRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.CreateSuscription(request));
            }
            return BadRequest();
        }

        [Route("api/v1/user/notifications_suscription")]
        [HttpPost]
        //[EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> NotificationsSuscription(NotificationRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _userService.NotificationsSuscription(request));
            }
            return BadRequest();
        }
    }
}
