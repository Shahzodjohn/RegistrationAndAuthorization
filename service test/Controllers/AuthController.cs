using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using service_test.DTOs;
using service_test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using service_test.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace service_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;


        public AuthController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
           
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dTO)
        {
            var userExist = await _userManager.FindByEmailAsync(dTO.Email);
            if (userExist != null)
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new Response { Status = "Error", Message = "User already exists! Duplication is prohibited!" });
            User user = new User()
            {
                FirstName = dTO.FirstName,
                LastName = dTO.LastName,
                MiddleName = dTO.MiddleName,
                UserName = dTO.Email,
                Email = dTO.Email,
                SecurityStamp = Guid.NewGuid().ToString() 
            };
            var result = await _userManager.CreateAsync(user, dTO.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "Registration  Error!" });
            }
            #region
            //if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            //    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            //if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            //    await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            //if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            //    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
#endregion
            return Ok(new Response { Status = "Success", Message = "User created sucessfully!"});
        }

        [HttpPost]
        [Route("Login")]
        
        public async Task<IActionResult> Login([FromBody] LoginDTO dTO)
        {
            var userExists = await _userManager.FindByEmailAsync(dTO.Email);
            if (userExists != null && await _userManager.CheckPasswordAsync(userExists, dTO.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(userExists);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userExists.Email),
                    new Claim(ClaimTypes.NameIdentifier, userExists.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidateAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256));
                var tokenJWT = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new
                {
                    Status = "Success",
                    token = "Bearer " + tokenJWT,
                    
                    Expiration = token.ValidTo
                });
            }
            return BadRequest(new
            {
                Status = "Bad request!",
                Error = "User is not registered!"
            });
        }
        [HttpGet("CurrentUser")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CurrentUser() 
        {
            var claim = this.User.Identity as ClaimsIdentity;
            var currentUser = await _userManager.GetUserAsync(User);
            var user = User.Identity.IsAuthenticated;
            var UserInfo = new UserDTO
            {
                Id = currentUser.Id,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                MiddleName = currentUser.MiddleName,
                Email = currentUser.Email,
                Password = "I think you dont have to see the password!"//currentUser.PasswordHash
                
            };
            var userEmail = claim.FindFirst(ClaimTypes.Name)?.Value;
            //var name = User.Identity.Name;
            if (UserInfo == null)
                return BadRequest(new Response { Message = "user not found" });
            return Ok( new 
            {
               User = UserInfo,
               IsAuthenticated = user
            });
        }
       
    }
}
