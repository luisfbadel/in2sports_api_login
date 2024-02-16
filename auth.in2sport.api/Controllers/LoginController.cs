using auth.in2sport.application.Services.LoginServices;
using auth.in2sport.application.Services.LoginServices.Requests;
using Microsoft.AspNetCore.Mvc;

namespace auth.in2sport.api.Controllers
{
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


        [Route("api/v1/signIn")]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.SignIn(request));
            }
            else return BadRequest();
        }

        [Route("api/v1/signUp")]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _loginService.SignUp(request));
            }
            else return BadRequest();
        }
    }
}
