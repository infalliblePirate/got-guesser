using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Exceptions;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GotExplorer.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(IJwtService jwtService, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<string> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username) ?? await _userManager.FindByEmailAsync(loginDTO.Username);

            if (user == null)
                throw new UnauthorizedException("Username not found and/or password incorrect");
           
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded) 
                throw new UnauthorizedException("Username not found and/or password incorrect");

            return _jwtService.GenerateToken(user);
        }

        public async Task<string> Register(RegisterDTO registerDTO)
        {
            var user = new User
            {
                UserName = registerDTO.Username,
                Email = registerDTO.Email,
            };
           
            var createdUser = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!createdUser.Succeeded)
                throw new IdentityException("User creation failed", createdUser.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded) 
                throw new IdentityException("Adding user role failed", roleResult.Errors);
            
            return _jwtService.GenerateToken(user);
        }
    }
}
