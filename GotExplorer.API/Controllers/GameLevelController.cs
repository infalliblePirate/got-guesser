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
    public class GameLevelController : ControllerBase
    {
        private readonly IGameLevelService _gameLevelService;

        public GameLevelController(IGameLevelService gameLevelService)
        {
            _gameLevelService = gameLevelService;
        }

        /// <summary>
        /// Calculate the score for a specific game and level.
        /// </summary>
        /// <response code="200">Score calculated successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Authentication failed due to invalid JWT.</response>
        /// <response code="403">User does not have sufficient permissions.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpPut("calculateScore")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(typeof(UpdateGameLevelDTO), 200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        [ProducesResponseType(typeof(ValidationResult), 401)]
        [ProducesResponseType(typeof(ValidationResult), 403)]
        [ProducesResponseType(typeof(ValidationResult), 500)]
        public async Task<IActionResult> CalculateScore([FromBody] CalculateScoreDTO calculateScoreDTO)
        {
            var userId = User.GetClaimValue("Id");
            var result = await _gameLevelService.CalculateScoreAsync(userId, calculateScoreDTO);
            return result.ToActionResult<UpdateGameLevelDTO>();
        }
    }
}