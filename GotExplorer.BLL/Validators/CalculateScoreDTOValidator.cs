using FluentValidation;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Options;
using GotExplorer.BLL.Services.Results;

namespace GotExplorer.BLL.Validators
{
    public class CalculateScoreDTOValidator : AbstractValidator<CalculateScoreDTO>
    {
        private readonly GameLevelOptions _gameLevelOptions;

        public CalculateScoreDTOValidator(GameLevelOptions gameLevelOptions)
        {
            _gameLevelOptions = gameLevelOptions;

            RuleFor(x => x.GameId)
                .GreaterThan(0)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage("GameId must be a positive integer.");

            RuleFor(x => x.LevelId)
                .GreaterThan(0)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage("LevelId must be a positive integer.");

            RuleFor(x => x.X)
                .InclusiveBetween(0, _gameLevelOptions.MaxX)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage($"X coordinate must be between 0 and {_gameLevelOptions.MaxX}.");

            RuleFor(x => x.Y)
                .InclusiveBetween(0, _gameLevelOptions.MaxY)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage($"Y coordinate must be between 0 and {_gameLevelOptions.MaxY}.");
        }
    }
}
