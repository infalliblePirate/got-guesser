using AutoMapper;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Exceptions;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.DAL.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly IMapper _mapper;

        public UserService(IJwtService jwtService, UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        public async Task<UserDTO> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username) ?? await _userManager.FindByEmailAsync(loginDTO.Username);
            
            if (user == null)
                throw new HttpException(StatusCodes.Status401Unauthorized,"Username not found and/or password incorrect");
           
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded)
                throw new HttpException(StatusCodes.Status401Unauthorized, "Username not found and/or password incorrect");

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Token = _jwtService.GenerateToken(user);
            return userDto;
        }

        public async Task<UserDTO> Register(RegisterDTO registerDTO)
        {         
            var user = _mapper.Map<User>(registerDTO);

            var createdUser = await _userManager.CreateAsync(user, registerDTO.Password);
            
            if (!createdUser.Succeeded)
            {
                var ex = new HttpException(StatusCodes.Status400BadRequest, "User creation failed");
                ex.Data["errors"] = createdUser.Errors;
                throw ex;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
            {
                var ex = new HttpException(StatusCodes.Status400BadRequest, "Adding user role failed");
                ex.Data["errors"] = roleResult.Errors;
                throw ex;
            }


            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Token = _jwtService.GenerateToken(user);
            return userDto;
        }
    }
}
