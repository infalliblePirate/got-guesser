using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Exceptions;
using GotExplorer.BLL.Services;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GotExplorer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _authService;
        public AccountController(IUserService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register new user.
        /// </summary>
        /// <param name="registerDTO">User data for registration.</param>
        /// <response code="200">Registration succeeded, and a JWT token is returned for the new user.</response>
        /// <response code="400">Registration failed due to invalid input.</response>
        /// <response code="500">An unexpected error occurred on the server</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var result = await _authService.Register(registerDTO);
                return Ok(result);
            }
            catch (IdentityException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="loginDTO">Login credentials</param>
        /// <response code="200">Authentication succeeded; returns a JWT token for the authenticated user.</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Authentication failed due to invalid username (or email) and/or password</response>
        /// <response code="500">An unexpected error occurred on the server</response>        
        [HttpPost("login")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.Login(loginDTO);
                return Ok(result);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // TODO: Delete endpoint.
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("JwtTest")]
        public IActionResult TestJwtAuth()
        {
            return Ok("Authentication successful");
        }
    }
}
