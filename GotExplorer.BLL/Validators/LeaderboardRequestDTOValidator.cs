using FluentValidation;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Results;

namespace GotExplorer.BLL.Validators
{
    public class LeaderboardRequestDTOValidator : AbstractValidator<LeaderboardRequestDTO>
    {
        public LeaderboardRequestDTOValidator()
        {
            RuleFor(x => x.Limit)
                .GreaterThanOrEqualTo(1)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage(ErrorMessages.InvalidLimit);
        }
    }
}