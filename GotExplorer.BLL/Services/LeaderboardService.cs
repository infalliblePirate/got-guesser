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
        private readonly UserManager<User> _userManager;
        private readonly IValidator<SubmitScoreDTO> _submitScoreValidator;
        private readonly IMapper _mapper;

        public LeaderboardService(AppDbContext appDbContext, UserManager<User> userManager, IValidator<SubmitScoreDTO> submitScoreValidator, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _submitScoreValidator = submitScoreValidator;
            _mapper = mapper;
        }

        public async Task<ValidationResult> SubmitScoreAsync(SubmitScoreDTO submitScoreDTO) 
        {
            var validationResult = await _submitScoreValidator.ValidateAsync(submitScoreDTO);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            var user = await _userManager.FindByIdAsync(submitScoreDTO.UserId);
            if (user == null)
            {
                return ValidationWithEntityModel<SubmitScoreDTO>.GenerateValidationFailure<SubmitScoreDTO>(
                    nameof(submitScoreDTO.UserId), ErrorMessages.IncorrectUserId, submitScoreDTO.UserId, ErrorCodes.Unauthorized);
            }

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                var userRecord = await _appDbContext.LeaderboardRecords.FirstOrDefaultAsync(e => e.UserId == user.Id);

                if (userRecord != null)
                {
                    userRecord.Score = submitScoreDTO.Score;
                    userRecord.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    userRecord = new LeaderboardRecord
                    {
                        UserId = user.Id,
                        Score = submitScoreDTO.Score,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _appDbContext.LeaderboardRecords.AddAsync(userRecord);
                }

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure(nameof(submitScoreDTO), ErrorMessages.LeaderboardServiceFailedToSaveScore, submitScoreDTO)
                        {
                            ErrorCode = ErrorCodes.LeaderboardUpdateFailed 
                        },
                    });
            }

            return new ValidationResult();
        }

        public async Task<ValidationWithEntityModel<List<LeaderboardRecordDTO>>> GetLeaderboardAsync(LeaderboardRequestDTO requestDTO)
        {
            try
            {
                var result = await _appDbContext.LeaderboardRecords
                    .OrderByDescending(e => e.Score)
                    .Take(requestDTO.Limit)
                    .ToListAsync();

                if (!result.Any())
                {
                    return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(
                        new ValidationFailure(nameof(requestDTO.Limit), ErrorMessages.LeaderboardServiceRecordsNotFound, requestDTO.Limit)
                        {
                            ErrorCode = ErrorCodes.NotFound
                        });
                }

                List<LeaderboardRecordDTO> listDTO = [.. result.Select(_mapper.Map<LeaderboardRecordDTO>)];
                return new ValidationWithEntityModel<List<LeaderboardRecordDTO>>(listDTO);
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