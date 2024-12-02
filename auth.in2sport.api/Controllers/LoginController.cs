using auth.in2sport.application.Services.LoginServices;
using auth.in2sport.application.Services.LoginServices.Requests;
using auth.in2sport.application.Services.UserServices.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace auth.in2sport.api.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {

        #region Private Properties

        /// <summary>
        /// Instance of the Login Service
        /// </summary>
        private readonly ILoginService _loginService;

        #endregion

        #region Constructor

        /// <summary>
        /// Defines constructor
        /// </summary>
        /// <param name="loginService"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }

        #endregion

        [Route("api/v1/login/signIn")]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.SignIn(request));
            }
            else return BadRequest();
        }

        [Route("api/v1/login/signUp")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.SignUp(request));
            }
            else return BadRequest();
        }

        //[Authorize]
        [Route("api/v1/login/user-registration")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> UserRegisteation(List<SignUpRequest> request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.UserRegistration(request));
            }
            else return BadRequest();
        }

        [Authorize]
        [Route("api/v1/login/update-password")]
        [HttpPatch]
        public async Task<IActionResult> UpdatePassword(UpdatePasswodRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.UpdatePassword(request));
            }
            else return BadRequest();
        }

        [Route("api/v1/login/get-refresh-token")]
        [HttpPost]
        //[EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetRefreshToken(RefreshTokenRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.GetRefreshToken(request));
            }
            else return BadRequest();
        }

        [Authorize]
        [Route("api/v1/login/validate-email")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> ValidateEmail(CodeKeyRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.ValidateEmail(request));
            }
            else return BadRequest();
        }

        [Route("api/v1/login/recover-password")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.RecoverPassword(request));
            }
            else return BadRequest();
        }

        [Route("api/v1/login/validate-code")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> ValidateCode(CodeKeyRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.ValidateCode(request));
            }
            else return BadRequest();
        }
    }
}
