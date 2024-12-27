using GotExplorer.API.Extensions;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services;
using GotExplorer.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Results;
namespace GotExplorer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }


        /// <summary>
        /// Start a new game.
        /// </summary>
        /// <response code="200">A new game was successfully created.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Authentication failed due to invalid JWT.</response>
        /// <response code="403">User does not have sufficient permissions.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>        
        [HttpPost("start")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(typeof(NewGameDTO), 200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        [ProducesResponseType(typeof(ValidationResult), 401)]
        [ProducesResponseType(typeof(ValidationResult), 403)]
        [ProducesResponseType(typeof(ValidationResult), 500)]
        public async Task<IActionResult> StartGame()
        {
            var userId = User.GetClaimValue("Id");
            var result = await _gameService.StartGameAsync(userId);
            return result.ToActionResult<NewGameDTO>();
        }
    }
}
