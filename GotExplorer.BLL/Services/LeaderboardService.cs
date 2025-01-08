using AutoMapper;
using FluentValidation;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.BLL.Services.Results;
using GotExplorer.BLL.Validators;
using GotExplorer.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using GotExplorer.DAL;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GotExplorer.BLL.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _appDbContext;

        public LeaderboardService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public async Task<ValidationWithEntityModel<List<LeaderboardRecordDTO>>> GetLeaderboardAsync(LeaderboardRequestDTO requestDTO)
        {
            try
            {
                var scores = await _appDbContext.Users
                    .Join(_appDbContext.Games.Where(game => game.EndTime != null),
                        user => user.Id,
                        game => game.UserId,
                        (user, game) => new { UserId = user.Id, GameId = game.Id })
                    .Join(_appDbContext.GameLevels,
                        userGame => userGame.GameId,
                        gl => gl.GameId,
                        (userGame, gl) => new { userGame.UserId, gl.Score })
                    .GroupBy(x => x.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        Score = g.Sum(x => x.Score ?? 0)
                    })
                    .Where(g => g.Score > 0)
                    .OrderByDescending(g => g.Score)
                    .Take(requestDTO.Limit)
                    .ToListAsync();

                var leaderboardRecords = scores
                    .Select(score => new LeaderboardRecordDTO
                    {
                        UserId = score.UserId,
                        Score = score.Score
                    })
                    .ToList();

                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(leaderboardRecords);
            }
            catch
            {
                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(
                    new ValidationFailure(nameof(requestDTO), ErrorMessages.LeaderboardServiceInternalError, requestDTO)
                    {
                        ErrorCode = ErrorCodes.InternalError
                    });
            }
        }
    }
}