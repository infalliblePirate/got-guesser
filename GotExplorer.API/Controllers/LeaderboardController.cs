using GotExplorer.API.Extensions;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services;
using GotExplorer.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using System.Collections;

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
        /// Retrieves the leaderboard, with optional query parameters for limiting, sorting and ordering the records. By default, it orders by score in descending order.
        /// </summary>
        /// <param name="requestDTO">The DTO containing query parameters to fetch leaderboard data.</param>
        /// <response code="200">The leaderboard data was successfully retrieved.</response>
        /// <response code="400">Invalid request data (e.g., invalid limit value).</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LeaderboardRecordDTO>),200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        [ProducesResponseType(typeof(ValidationResult), 500)]
        public async Task<IActionResult> GetLeaderboard([FromQuery] LeaderboardRequestDTO requestDTO)
        {
            var result = await _leaderboardService.GetLeaderboardAsync(requestDTO);
            return result.ToActionResult();
        }
    }
}