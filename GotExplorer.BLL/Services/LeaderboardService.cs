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
using System.Linq;

namespace GotExplorer.BLL.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<LeaderboardRequestDTO> _leaderboardRequestValidator;
        private readonly IValidator<LeaderboardUserRequestDTO> _leaderboardUserRequestValidator;
        public LeaderboardService(AppDbContext appDbContext, 
            IValidator<LeaderboardRequestDTO> leaderboardRequestValidator, 
            IValidator<LeaderboardUserRequestDTO> leaderboardUserRequestValidator, 
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _leaderboardRequestValidator = leaderboardRequestValidator;
            _leaderboardUserRequestValidator = leaderboardUserRequestValidator;
            _mapper = mapper;   
        }

        public async Task<ValidationWithEntityModel<List<LeaderboardRecordDTO>>> GetLeaderboardAsync(LeaderboardRequestDTO requestDTO)
        {
            var validationResult = await _leaderboardRequestValidator.ValidateAsync(requestDTO);
            if (!validationResult.IsValid )
            {
                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(validationResult);
            }

            var leaderboard = GetLeaderboardQuery(requestDTO.SortBy, requestDTO.OrderBy, requestDTO.Limit).ToList();

            return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(leaderboard);
        }

        public async Task<ValidationWithEntityModel<LeaderboardUserDTO>> GetUserLeaderboardAsync(LeaderboardUserRequestDTO requestDTO)
        {
            var validationResult = await _leaderboardUserRequestValidator.ValidateAsync(requestDTO);
            if (!validationResult.IsValid)
            {
                return new ValidationWithEntityModel<LeaderboardUserDTO>(validationResult);
            }

            var leaderboardRecords = GetLeaderboardQuery(requestDTO.SortBy,requestDTO.OrderBy).AsEnumerable();

            var leaderboardUserRecord = leaderboardRecords
                .Where(g => g.UserId == requestDTO.UserId)
                .FirstOrDefault();
           
            if (leaderboardUserRecord == null)
            {
                return new ValidationWithEntityModel<LeaderboardUserDTO>(
                    new ValidationFailure(nameof(requestDTO.UserId),ErrorMessages.UserServiceUserNotFound) { ErrorCode = ErrorCodes.NotFound}
                );
            }

            var userRecord = _mapper.Map<LeaderboardUserDTO>(leaderboardUserRecord);
            userRecord.Rank = CalculateRank(leaderboardRecords, userRecord, requestDTO.SortBy,requestDTO.OrderBy);

            return new ValidationWithEntityModel<LeaderboardUserDTO>(userRecord);
        }

        private IQueryable<LeaderboardRecordDTO?> GetLeaderboardQuery(LeaderboardSortBy sortBy, OrderBy orderBy, int? limit=null)
        {
            var query = _appDbContext.Games
                .Where(game => game.EndTime != null)
                .Select(x => new LeaderboardRecordDTO()
                {
                    UserId = x.UserId,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime.Value,
                    Score = _appDbContext.GameLevels
                     .Where(gl => gl.GameId == x.Id)
                     .Sum(gl => gl.Score ?? 0),
                })
               .GroupBy(x => x.UserId)
               .OrderByDynamic(GetSortBy(sortBy), orderBy)
               .Select(g => g.OrderByDescending(x => x.Score).First())
               .TakeOrDefault(limit);

            return query;
                
        }

        private Expression<Func<IEnumerable<LeaderboardRecordDTO>, object>> GetSortBy(LeaderboardSortBy sortBy)
        {
            return sortBy switch
            {
                LeaderboardSortBy.Time => x => x.Min(s => s.EndTime) - x.Min(s => s.StartTime),
                LeaderboardSortBy.Score => x => x.Min(s => s.Score),
            };
        }

        private int CalculateRank(IEnumerable<LeaderboardRecordDTO?> leaderboardRecords, LeaderboardUserDTO userRecord, LeaderboardSortBy sortBy, OrderBy orderBy)
        {
            if (sortBy == LeaderboardSortBy.Score)
            {
                if (orderBy == OrderBy.Desc)
                {
                    return leaderboardRecords.Count(g => g.Score > userRecord.Score) + 1;
                }
                return leaderboardRecords.Count(g => g.Score < userRecord.Score) + 1;
            }
            else
            {
                var userTime = userRecord.EndTime - userRecord.StartTime;

                if (orderBy == OrderBy.Asc)
                {
                    return leaderboardRecords.Count(g => g.EndTime - g.StartTime < userTime) + 1;
                }
                return leaderboardRecords.Count(g => g.EndTime - g.StartTime > userTime) + 1;
                
            }
        }

    }
}