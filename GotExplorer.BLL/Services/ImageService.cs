﻿using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Interfaces;
using GotExplorer.BLL.Services.Results;
using GotExplorer.DAL;
using Microsoft.AspNetCore.Hosting;
using FluentValidation.Results;
using AutoMapper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using System.IO;
using GotExplorer.DAL.Entities;
using FluentValidation;
using GotExplorer.BLL.Validators;
namespace GotExplorer.BLL.Services
{
    public class ImageService : IImageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imagePath = "Images";
        private readonly IValidator<UploadImageDTO> _imageValidator;
        public ImageService(AppDbContext appDbContext,IMapper mapper, IWebHostEnvironment webHostEnvironment, IValidator<UploadImageDTO> imageValidator)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _imageValidator = imageValidator;
        }
            
        public async Task<ServiceResult<ImageDTO>> GetImageAsync(Guid id)
        {
            var serviceResult = new ServiceResult<ImageDTO>();
            var result = await _appDbContext.Images.FindAsync(id);

            if (result == null || !File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, result.Path)))
            {
                serviceResult.Error = new Error(ErrorCodes.NotFound, new ValidationResult
                {
                    Errors = new() {
                        new ValidationFailure(nameof(id), "Image not found", id),
                    }
                });
                return serviceResult;
            }
            var image = _mapper.Map<ImageDTO>(result);
            serviceResult.ResultObject = image;
            return serviceResult;
        }

        public async Task<ServiceResult<IEnumerable<ImageDTO>>> GetAllImagesAsync()
        {
            var serviceResult = new ServiceResult<IEnumerable<ImageDTO>>();
            serviceResult.ResultObject = await _appDbContext.Images.Select(x => _mapper.Map<ImageDTO>(x)).ToListAsync();
            return serviceResult;
        }

        public async Task<ServiceResult> UploadImageAsync(UploadImageDTO uploadImageDTO)
        {
            var serviceResult = new ServiceResult();
            var validationResult = await _imageValidator.ValidateAsync(uploadImageDTO);
            if (!validationResult.IsValid)
            {
                serviceResult.Error = new Error(ErrorCodes.Invalid, validationResult);
                return serviceResult;
            }
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadImageDTO.Image.FileName)}";
            var filePath = Path.Combine(_imagePath,fileName);
            var fileAbsolutePath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fileAbsolutePath));
            using (var stream = new FileStream(fileAbsolutePath, FileMode.Create))
            {
                await uploadImageDTO.Image.CopyToAsync(stream);
            }

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                _appDbContext.Add(new Image()
                {
                    Name = uploadImageDTO.Image.FileName,
                    Path = filePath
                });
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                if (File.Exists(fileAbsolutePath)) { 
                    File.Delete(fileAbsolutePath);
                }

                serviceResult.Error = new Error(ErrorCodes.ImageUploadFailed, new ValidationResult
                {
                    Errors = new() {
                        new ValidationFailure(nameof(uploadImageDTO.Image), "Failed to upload the image"),
                    }
                });
            }
            return serviceResult;
        }

        public async Task<ServiceResult> UpdateImageAsync(Guid id, UploadImageDTO uploadImageDTO)
        {
            var serviceResult = new ServiceResult();

            var validationResult = await _imageValidator.ValidateAsync(uploadImageDTO);
            if (!validationResult.IsValid)
            {
                serviceResult.Error = new Error(ErrorCodes.Invalid, validationResult);
                return serviceResult;
            }

            var image = await _appDbContext.Images.FindAsync(id);

            if (image == null)
            {
                serviceResult.Error = new Error(ErrorCodes.NotFound, new ValidationResult
                {
                    Errors = new() {
                        new ValidationFailure(nameof(id), "Image not found", id)
                    }
                });
                return serviceResult;
            }

            var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadImageDTO.Image.FileName)}";
            var newFilePath = Path.Combine(_imagePath, newFileName);
            var newFileAbsolutePath = Path.Combine(_webHostEnvironment.WebRootPath, newFilePath);

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newFileAbsolutePath));
                using (var stream = new FileStream(newFileAbsolutePath, FileMode.Create))
                {
                    await uploadImageDTO.Image.CopyToAsync(stream);
                }
                var oldFileAbsolutePath = Path.Combine(_webHostEnvironment.WebRootPath, image.Path);

                image.Name = uploadImageDTO.Image.FileName;
                image.Path = newFilePath;
                _appDbContext.Images.Update(image);

                await _appDbContext.SaveChangesAsync();

                if (File.Exists(oldFileAbsolutePath))
                {
                    File.Delete(oldFileAbsolutePath);
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                if (File.Exists(newFileAbsolutePath))
                {
                    File.Delete(newFileAbsolutePath);
                }

                serviceResult.Error = new Error(ErrorCodes.ImageUpdateFailed, new ValidationResult
                {
                    Errors = new()
                    {
                        new ValidationFailure(nameof(uploadImageDTO.Image), "Failed to update the image"),
                    }
                });
            }

            return serviceResult;
        }

        public async Task<ServiceResult> DeleteImageAsync(Guid id)
        {
            var serviceResult = new ServiceResult();

            var image = await _appDbContext.Images.FindAsync(id);
            if (image == null)
            {
                serviceResult.Error = new Error(ErrorCodes.NotFound, new ValidationResult
                {
                    Errors = new()
                    {
                        new ValidationFailure(nameof(id), "Image not found", id)
                    }
                });
                return serviceResult;
            }

            var fileAbsolutePath = Path.Combine(_webHostEnvironment.WebRootPath, image.Path);
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                _appDbContext.Images.Remove(image);
                await _appDbContext.SaveChangesAsync();

                if (File.Exists(fileAbsolutePath))
                {
                    File.Delete(fileAbsolutePath);
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                serviceResult.Error = new Error(ErrorCodes.ImageDeletionFailed, new ValidationResult
                {
                    Errors = new()
                    {
                        new ValidationFailure(nameof(id), "Failed to delete the image")
                    }
                });
            }

            return serviceResult;
        }

    }
}
