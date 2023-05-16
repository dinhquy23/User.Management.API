using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.Login;
using User.Management.API.Models.Authentication.SignUp;
using User.Management.Service.Models;
using User.Management.Service.Services;

namespace User.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailServiceCustom _emailService;
        private readonly IConfiguration _configuration;

        public AuthenticationController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailServiceCustom emailService,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _signInManager = signInManager;
        }
        /// <summary>
        /// Register User
        /// </summary>
        /// <param name="register"></param>
        /// <param name="role"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUser register, string role)
        {
            // check user exist
            var userExist = await _userManager.FindByEmailAsync(register.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Responsive { Status = "Error", Message = "User already exist!" });
            }

            // init user
            IdentityUser user = new()
            {
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = register.Username,
                TwoFactorEnabled = true
            };
            if (await _roleManager.RoleExistsAsync(role))
            {
                // add user to database
                var addUser = await _userManager.CreateAsync(user, register.Password);
                if (!addUser.Succeeded)
                {
                    var error = addUser.Errors.FirstOrDefault()?.Description;
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new Responsive { Status = "Error", Message = $"{error} User Failed To Create!" });
                }

                // add role to user
                var addRole = await _userManager.AddToRoleAsync(user, role);

                // add token to verify email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email }, Request.Scheme);
                var message = new Message(new string[] { user.Email! }, "Confirm email link", confirmationLink!);
                _emailService.SendEmail(message);

                if (!addRole.Succeeded)
                {
                    var error = addRole.Errors.FirstOrDefault()?.Description;
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new Responsive { Status = "Error", Message = $"{error} Failed To Create Role For User!" });
                }

                return StatusCode(StatusCodes.Status200OK,
                    new Responsive { Status = "Success", Message = $"User create successful, go to {user.Email} and verify this email" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Responsive { Status = "Error", Message = "This Role Doesn't Exist!" });
            }

        }

        //[HttpGet]
        //public IActionResult TestSendEmail()
        //{
        //    var message = new Message(
        //        new string[] { "quy.nguyen@ntq-solution.com.vn" }, 
        //        "[MAI TU DONG TU DINH QUY]", "Dear may,\nMai la thu 7 nen tao xin nghi may nhe.\nCam on.");

        //    _emailService.SendEmail(message);
        //    return StatusCode(StatusCodes.Status200OK, new Responsive { Status = "Success", Message = "Send email success!" });
        //}

        /// <summary>
        /// Confirm Email
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, new Responsive { Status = "Success", Message = "Email Verify Successfully" });
                }
                else
                {
                    var error = result.Errors.FirstOrDefault()?.Description;
                    return StatusCode(StatusCodes.Status200OK, new Responsive { Status = "Error", Message = $"{error} Fail To Verify Email" });
                }
            }
            return StatusCode(StatusCodes.Status200OK, new Responsive { Status = "Error", Message = "User Doesn't Exist" });
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            //checking user
            var user = await _userManager.FindByNameAsync(loginModel.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                if (user.TwoFactorEnabled)
                {
                    await _signInManager.SignOutAsync();
                    await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
                    var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    var message = new Message(new string[] { user.Email! }, "OTP Confirmation", $"Confirmation code is: {code}");
                    _emailService.SendEmail(message);
                    return StatusCode(StatusCodes.Status200OK, new Responsive { Status = "Success", Message = $"We have sent an OTP to your Email {user.Email}" });
                }
                else
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };
                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var jwtToken = GetToken(authClaims);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        expiration = jwtToken.ValidTo
                    });
                }
            }
            return Unauthorized();
        }

        /// <summary>
        /// Login Two Factor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login-2FA")]
        public async Task<IActionResult> LoginWithOtp(string code, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var signIn = await _signInManager.TwoFactorSignInAsync("Email", code, false, false);
            if (signIn.Succeeded)
            {
                if (user != null)
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };
                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var jwtToken = GetToken(authClaims);


                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        expiration = jwtToken.ValidTo
                    });
                }
            }

            return StatusCode(StatusCodes.Status404NotFound, new Responsive { Status = "Error", Message = $"Invalid code" });
        }

        /// <summary>
        /// Get Token
        /// </summary>
        /// <param name="authClaims"></param>
        /// <returns></returns>
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningInKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
    }
}
