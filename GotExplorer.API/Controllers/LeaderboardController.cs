using GotExplorer.API.Extensions;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services;
using GotExplorer.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;

namespace GotExplorer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        /// <summary>
        /// Submits a player's score to the leaderboard. Requires authorization.
        /// </summary>
        /// <param name="submitScoreDTO">The DTO containing the score and user information to be submitted.</param>
        /// <response code="200">The score was successfully submitted.</response>
        /// <response code="400">Invalid request data (e.g., missing score or invalid user data).</response>
        /// <response code="401">Authentication failed due to invalid or missing JWT.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        /// [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        [ProducesResponseType(typeof(ValidationResult), 401)]
        [ProducesResponseType(typeof(ValidationResult), 500)]
        public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreDTO submitScoreDTO)
        {
            submitScoreDTO.UserId = User.GetClaimValue("Id");
            var result = await _leaderboardService.SubmitScoreAsync(submitScoreDTO);
            return result.ToActionResult();
        }

        /// <summary>
        /// Retrieves the leaderboard, with the option to limit the number of records. 
        /// </summary>
        /// <param name="requestDTO">The DTO containing query parameters such as the limit for the number of leaderboard records to fetch.</param>
        /// <response code="200">The leaderboard data was successfully retrieved.</response>
        /// <response code="400">Invalid request data (e.g., invalid limit value).</response>
        /// <response code="404">No leaderboard records found for the given limit.</response>
        /// <response code="409">The score update failed due to a conflict with another modification of the leaderboard record.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        [ProducesResponseType(typeof(ValidationResult), 401)]
        [ProducesResponseType(typeof(ValidationResult), 404)]
        [ProducesResponseType(typeof(ValidationResult), 409)]
        [ProducesResponseType(typeof(ValidationResult), 500)]
        public async Task<IActionResult> GetLeaderboard([FromQuery] LeaderboardRequestDTO requestDTO)
        {
            var result = await _leaderboardService.GetLeaderboardAsync(requestDTO);
            return result.ToActionResult();
        }
    }
}