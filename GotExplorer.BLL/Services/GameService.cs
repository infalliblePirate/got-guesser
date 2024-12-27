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
    public class GameService : IGameService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<CompleteGameDTO> _completeGameValidator;
        private readonly UserManager<User> _userManager;
        private readonly GameOptions _gameOptions;

        public GameService(IOptions<GameOptions> gameOptions, AppDbContext appDbContext, UserManager<User> userManager, IValidator<CompleteGameDTO> completeGameValidator)
        {
            _gameOptions = gameOptions.Value;
            _appDbContext = appDbContext;
            _completeGameValidator = completeGameValidator;
            _userManager = userManager;
        }

        public async Task<ValidationResult> CompleteGameAsync(CompleteGameDTO completeGameDTO)
        {
            var validationResult = await _completeGameValidator.ValidateAsync(completeGameDTO);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            var game = await _appDbContext.Games
                .Include(g => g.Levels)
                .FirstOrDefaultAsync(g => g.Id == completeGameDTO.GameId && g.UserId == completeGameDTO.UserId);

            if (game == null)
            {
                return new ValidationResult(
                    [
                        new ValidationFailure(nameof(completeGameDTO.GameId), "Game not found or already completed.")
                        {
                            ErrorCode = ErrorCodes.GameNotFound
                        }
                    ]);
            }

            //game.Score = game.Levels.Count * 10; // TODO: hardcoded

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                _appDbContext.Games.Update(game);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ValidationResult(
                    [
                        new ValidationFailure(nameof(completeGameDTO.GameId), "Failed to complete the game.")
                        {
                            ErrorCode = ErrorCodes.GameCompletionFailed
                        }
                    ]);
            }

            return new ValidationResult();
        }

        public async Task<ValidationWithEntityModel<NewGameDTO>> StartGameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ValidationWithEntityModel<NewGameDTO>(
                    new ValidationFailure(nameof(userId), ErrorMessages.IncorrectUserId, userId) { ErrorCode = ErrorCodes.Unauthorized }
                );
            }

            var levels = _appDbContext.Levels.OrderBy(r => Guid.NewGuid()).Take(_gameOptions.LevelsPerGame).ToList();

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            var newGameDto = new NewGameDTO();
            newGameDto.LevelIds = levels.Select(e => e.Id);

            try
            {
                _appDbContext.Attach(user);
                _appDbContext.AttachRange(levels);
                var game = new Game()
                {
                    User = user,
                    StartTime = DateTime.UtcNow,
                    GameType = GameType.Standard,
                    Levels = levels
                };

                await _appDbContext.Games.AddAsync(game);
                await _appDbContext.SaveChangesAsync();
                newGameDto.GameId = game.Id;
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ValidationWithEntityModel<NewGameDTO>(
                    [
                        new ValidationFailure()
                        {
                            ErrorMessage = ErrorMessages.FailedToStartTheGame,
                            ErrorCode = ErrorCodes.GameStartFailed
                        }
                    ]);
            }

            return new ValidationWithEntityModel<NewGameDTO>(newGameDto);
        }
    }
}
