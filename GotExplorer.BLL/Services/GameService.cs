using AutoMapper;
using FluentValidation;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.BLL.Services.Results;
using GotExplorer.DAL;
using GotExplorer.DAL.Entities;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace GotExplorer.BLL.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<CompleteGameDTO> _completeGameValidator;

        public GameService(AppDbContext appDbContext, IValidator<CompleteGameDTO> completeGameValidator)
        {
            _appDbContext = appDbContext;
            _completeGameValidator = completeGameValidator;
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

            game.Score = game.Levels.Count * 10; // TODO: hardcoded

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

    }
}
