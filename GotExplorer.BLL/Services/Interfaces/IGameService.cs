using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Results;
using FluentValidation.Results;

namespace GotExplorer.BLL.Services.Interfaces
{
    public interface IGameService
    {
        Task<ValidationResult> CompleteGameAsync(CompleteGameDTO completeGameDTO);
        Task<ValidationWithEntityModel<NewGameDTO>> StartGameAsync(string userId);
    }
}