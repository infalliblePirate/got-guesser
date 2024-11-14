using GotExplorer.BLL.DTOs;

namespace GotExplorer.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> Register(RegisterDTO registerDTO);
        Task<string> Login(LoginDTO loginDTO);
    }
}
