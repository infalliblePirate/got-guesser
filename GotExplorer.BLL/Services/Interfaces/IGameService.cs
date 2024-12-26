using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Results;
using FluentValidation.Results;

namespace GotExplorer.BLL.Services.Interfaces
{
    public interface IGameService
    {
        public Task<ValidationResult> CompleteGameAsync(CompleteGameDTO completeGameDTO);
    }
}