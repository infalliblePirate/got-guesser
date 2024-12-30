using AutoMapper;
using FluentValidation;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.BLL.Services.Results;
using GotExplorer.DAL;
using GotExplorer.DAL.Entities;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GotExplorer.BLL.Options;
using Microsoft.Extensions.Options;


namespace GotExplorer.BLL.Services
{
    public class GameLevelService : IGameLevelService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly IValidator<CalculateScoreDTO> _calculateScoreValidator;
        private readonly GameLevelOptions _gameLevelOptions;
        private readonly IMapper _mapper;

        public GameLevelService(IOptions<GameLevelOptions> gameLevelOptions, AppDbContext appDbContext, UserManager<User> userManager, IValidator<CalculateScoreDTO> calculateScoreValidator, IMapper mapper)
        {
            _gameLevelOptions = gameLevelOptions.Value;
            _appDbContext = appDbContext;
            _calculateScoreValidator = calculateScoreValidator;
            _userManager = userManager;
            _mapper = mapper;
        }

         public async Task<ValidationWithEntityModel<UpdateGameLevelDTO>> CalculateScoreAsync(string userId, CalculateScoreDTO calculateScoreDTO)
        {
            var validationResult = await _calculateScoreValidator.ValidateAsync(calculateScoreDTO);
            if (!validationResult.IsValid)
            {
                return new ValidationWithEntityModel<UpdateGameLevelDTO>(validationResult);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return GenerateValidationFailure(nameof(userId), ErrorMessages.IncorrectUserId, userId, ErrorCodes.Unauthorized);
            }

            var game = await _appDbContext.Games.FindAsync(calculateScoreDTO.GameId);
            if (game == null)
            {
                return GenerateValidationFailure(nameof(calculateScoreDTO.GameId), ErrorMessages.GameServiceGameNotFound, calculateScoreDTO.GameId, ErrorCodes.NotFound);
            }

            if (game.UserId != user.Id)
            {
                return GenerateValidationFailure(nameof(calculateScoreDTO.GameId), ErrorMessages.IncorrectUserId, calculateScoreDTO.GameId, ErrorCodes.Unauthorized);
            }

            var gameLevel = await _appDbContext.GameLevels
                .Where(gl => gl.GameId == calculateScoreDTO.GameId && gl.LevelId == calculateScoreDTO.LevelId)
                .FirstOrDefaultAsync();
            
            if (gameLevel == null)
            {
                return GenerateValidationFailure(nameof(calculateScoreDTO.LevelId), ErrorMessages.GameLevelServiceGameLevelNotFound, calculateScoreDTO.LevelId, ErrorCodes.NotFound);
            }

            var requestedLevel = await _appDbContext.Levels.FindAsync(calculateScoreDTO.LevelId);
            if (requestedLevel == null)
            {
                return GenerateValidationFailure(nameof(calculateScoreDTO.LevelId), ErrorMessages.LevelServiceLevelNotFound, calculateScoreDTO.LevelId, ErrorCodes.NotFound);
            }

            int score = CalculateLevelScore(requestedLevel.X, requestedLevel.Y, calculateScoreDTO.X, calculateScoreDTO.Y);

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                gameLevel.Score = score;
                _appDbContext.GameLevels.Update(gameLevel);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var updatedGameLevelDTO = _mapper.Map<UpdateGameLevelDTO>(gameLevel);
                return new ValidationWithEntityModel<UpdateGameLevelDTO>(updatedGameLevelDTO);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ValidationWithEntityModel<UpdateGameLevelDTO>(
                    new ValidationFailure(nameof(calculateScoreDTO.LevelId), ErrorMessages.GameLevelServiceFailedToSaveScore)
                    {
                        ErrorCode = ErrorCodes.GameLevelUpdateFailed
                    });
            }
        }

        private ValidationWithEntityModel<UpdateGameLevelDTO> GenerateValidationFailure(string fieldName, string errorMessage, object attemptedValue, string errorCode)
        {
            return new ValidationWithEntityModel<UpdateGameLevelDTO>(
                new ValidationFailure(fieldName, errorMessage, attemptedValue)
                {
                    ErrorCode = errorCode
                });
        }

        private double CalculateDistance(double correctX, double correctY, double chosedX, double chosedY)
        {
            return Math.Sqrt(Math.Pow(chosedX - correctX, 2) + Math.Pow(chosedY - correctY, 2));
        }

        private int CalculateLevelScore(double correctX, double correctY, double chosedX, double chosedY)
        {
            double distance = CalculateDistance(correctX, correctY, chosedX, chosedY);

            if (distance <= _gameLevelOptions.ProximityRadius)
                return _gameLevelOptions.ScoreWithinRadius;

            double excessDistance = distance - _gameLevelOptions.ProximityRadius;
            double penalty = excessDistance / _gameLevelOptions.PenaltyStep * _gameLevelOptions.PenaltyPerStep;
            return Math.Max(0, _gameLevelOptions.ScoreWithinRadius - (int)Math.Round(penalty));
        }

    }
}