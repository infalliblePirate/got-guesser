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
using GotExplorer.BLL.Extensions;
using System.Linq.Expressions;
using System;

namespace GotExplorer.BLL.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<LeaderboardRequestDTO> _leaderboardRequestValidator;
        public LeaderboardService(AppDbContext appDbContext, IValidator<LeaderboardRequestDTO> leaderboardRequestValidator)
        {
            _appDbContext = appDbContext;
            _leaderboardRequestValidator = leaderboardRequestValidator;
        }


        public async Task<ValidationWithEntityModel<List<LeaderboardRecordDTO>>> GetLeaderboardAsync(LeaderboardRequestDTO requestDTO)
        {
            var validationResult = await _leaderboardRequestValidator.ValidateAsync(requestDTO);
            if (!validationResult.IsValid )
            {
                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(validationResult);
            }
            try
            {
                var leaderboard = _appDbContext.Games
                    .Where(game => game.EndTime != null)
                    .Join(_appDbContext.GameLevels,
                        game => game.Id,
                        gl => gl.GameId,
                        (game, gl) => new { game.UserId, gl.Score, game.StartTime, game.EndTime})
                    .GroupBy(x => new { x.UserId, x.StartTime, x.EndTime })
                    .Select(g => new LeaderboardRecordDTO
                    {
                        UserId = g.Key.UserId,
                        Score = g.Sum(x => x.Score ?? 0),
                        StartTime = g.Key.StartTime,
                        EndTime = g.Key.EndTime!.Value,
                    })
                    .Where(g => g.Score > 0)
                    .AsEnumerable()
                    .GroupBy(g => g.UserId)
                    .Select(g => g.MaxBy(x => x.Score))
                    .OrderByDynamic(GetSortBy(requestDTO.SortBy), requestDTO.OrderBy)
                    .TakeOrDefault(requestDTO.Limit)
                    .ToList();

                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(leaderboard);
            }
            catch (Exception ex)
            {
                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(
                    new ValidationFailure(nameof(requestDTO), ErrorMessages.LeaderboardServiceInternalError, requestDTO)
                    {
                        ErrorCode = ErrorCodes.InternalError
                    });
            }
        }

        private Func<LeaderboardRecordDTO, object> GetSortBy(LeaderboardSortBy sortBy)
        {
            return sortBy switch
            {
                LeaderboardSortBy.Time => x => x.EndTime - x.StartTime,
                LeaderboardSortBy.Score => x => x.Score,
            };
        }
    }
}