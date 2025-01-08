using FluentValidation;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Results;
using Microsoft.AspNetCore.Rewrite;

namespace GotExplorer.BLL.Validators
{
    public class SubmitScoreDTOValidator : AbstractValidator<SubmitScoreDTO>
    {
        public SubmitScoreDTOValidator()
        {
            RuleFor(x => x.UserId)
                .Must(x => int.TryParse(x, out var val) && val > 0)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage(ErrorMessages.IncorrectUserId);

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(ErrorCodes.Invalid)
                .WithMessage(ErrorMessages.InvalidScore);
        }
    }
}