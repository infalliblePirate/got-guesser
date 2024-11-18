using GotExplorer.BLL.DTOs;

namespace GotExplorer.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> Register(RegisterDTO registerDTO);
        Task<UserDTO> Login(LoginDTO loginDTO);
    }
}
