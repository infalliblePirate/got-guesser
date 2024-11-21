using GotExplorer.BLL.DTOs;

namespace GotExplorer.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> Register(RegisterDTO registerDTO);
        Task<UserDTO> Login(LoginDTO loginDTO);
        Task UpdateUserById(UpdateUserDTO updateUserDTO);
        Task DeleteUserById(string userId);
        Task UpdatePassword(UpdateUserPasswordDTO updateUserPasswordDTO);
        Task<GetUserDTO> GetUserById(string id);
        Task GeneratePasswordResetLink(GeneratePasswordResetLinkDTO generatePasswordResetLinkDTO, string origin);
        Task ResetPassword(ResetPasswordDTO ResetPasswordDTO);
    }
}
