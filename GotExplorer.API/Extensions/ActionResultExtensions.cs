﻿using GotExplorer.BLL.Services.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections;
using System;
using FluentValidation.Results;

namespace GotExplorer.API.Extensions
{
    public static class ActionResultExtensions
    {
        public static IActionResult ToActionResult<TData>(this ValidationWithEntityModel<TData> validationResult)
        {
            if (validationResult.IsValid)
            {
                return new OkObjectResult(validationResult.Entity);
            }
            return GetActionResult(validationResult);
        }
        public static IActionResult ToActionResult(this ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                return new OkResult();
            }
            return GetActionResult(validationResult);
        }
        private static IActionResult GetActionResult(ValidationResult validationResult)
        {
            var result = new ObjectResult(validationResult);

            var countUniqueErrors = validationResult.Errors
                .Select(e => e.ErrorCode)
                .Distinct().Count();

            if (countUniqueErrors > 1)
            {
                result.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return result;
            }

            result.StatusCode = validationResult.Errors[0].ErrorCode switch
            {
                ErrorCodes.NotFound => StatusCodes.Status404NotFound,
                ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorCodes.Invalid => StatusCodes.Status400BadRequest,
                ErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
                ErrorCodes.UserCreationFailed => StatusCodes.Status400BadRequest,
                ErrorCodes.RoleAssignmentFailed => StatusCodes.Status500InternalServerError,
                ErrorCodes.UserUpdateFailed => StatusCodes.Status400BadRequest,
                ErrorCodes.UserPasswordUpdateFailed => StatusCodes.Status400BadRequest,
                ErrorCodes.UserResetPasswordFailed => StatusCodes.Status400BadRequest,
                ErrorCodes.UserDeletionFailed => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError,
            };
            return result;
        }
    }
}
