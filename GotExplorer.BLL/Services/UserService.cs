using AutoMapper;
using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Exceptions;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace GotExplorer.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserService(IJwtService jwtService, UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, IEmailService emailService)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _emailService = emailService;
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

        public async Task<GetUserDTO> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                throw new HttpException(StatusCodes.Status404NotFound, "User not found");

            return _mapper.Map<GetUserDTO>(user);
        }
             
        public async Task UpdateUserById(UpdateUserDTO updateUserDTO)
        {
            var user = await _userManager.FindByIdAsync(updateUserDTO.Id);

            if (user == null)
                throw new HttpException(StatusCodes.Status404NotFound, "User not found");

            _mapper.Map(updateUserDTO,user);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var ex = new HttpException(StatusCodes.Status400BadRequest, "Updating user failed");
                ex.Data["errors"] = result.Errors;
                throw ex;
            }
        }

        public async Task UpdatePassword(UpdateUserPasswordDTO updateUserPasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(updateUserPasswordDTO.Id);

            if (user == null)
                throw new HttpException(StatusCodes.Status404NotFound, "User not found");

            var result = await _userManager.ChangePasswordAsync(user,updateUserPasswordDTO.CurrentPassword,updateUserPasswordDTO.NewPassword);

            if (!result.Succeeded)
            {
                var ex = new HttpException(StatusCodes.Status400BadRequest, "Updating password failed");
                ex.Data["errors"] = result.Errors;
                throw ex;
            }
        }

        public async Task GeneratePasswordResetLink(GeneratePasswordResetLinkDTO generatePasswordResetLinkDTO, string origin)
        {
            var user = await _userManager.FindByEmailAsync(generatePasswordResetLinkDTO.Email);

            if (user == null)
                throw new HttpException(StatusCodes.Status404NotFound, "User not found");
   
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = $"{origin}reset-password?id={user.Id}&token={token}";
            _emailService.SendEmail(generatePasswordResetLinkDTO.Email, "Your Password Reset Link", url);
        }

        public async Task ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(resetPasswordDTO.Id.ToString());
            if (user == null)
                throw new HttpException(StatusCodes.Status404NotFound, "User not found");

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.Token);

            if (!result.Succeeded)
            {
                var ex = new HttpException(StatusCodes.Status400BadRequest, "Resetting password failed");
                ex.Data["errors"] = result.Errors;
                throw ex;
            }
        }

        public async Task DeleteUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new HttpException(StatusCodes.Status404NotFound, "User not found");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var ex = new HttpException(StatusCodes.Status400BadRequest, "Deleting user failed");
                ex.Data["errors"] = result.Errors;
                throw ex;
            }
        }
    }
}
