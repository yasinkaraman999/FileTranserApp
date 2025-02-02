using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTransferApp.Data;
using FileTransferApp.Data.Models;
using FileTransferApp.Models;
using FileTransferApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace FileTransferApp.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileService(
            ApplicationDbContext context, 
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _uploadPath = Path.Combine(env.WebRootPath, "uploads");
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<FileResponseModel> UploadFileAsync(IFormFile file, string? password = null, DateTime? expiryDate = null)
        {
            var uploadDto = new FileUploadModel
            {
                Password = password,
                ExpiryDate = expiryDate,
                IsPublic = true
            };
            return await UploadFileAsync(file, uploadDto);
        }

        public async Task<FileResponseModel> GetFileInfoAsync(Guid id)
        {
            return await GetFileByIdAsync(id);
        }

        public async Task<List<FileResponseModel>> GetAllFilesAsync(bool includeDeleted = false)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            
            if (user == null)
                return new List<FileResponseModel>();

            var query = _context.Files.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(f => !f.ExpiryDate.HasValue || f.ExpiryDate > DateTime.Now);
            }

            // Sadece kullanıcının kendi dosyaları ve herkese açık dosyalar
            query = query.Where(f => f.UserId == user.Id || f.IsPublic);

            var files = await query
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            return files.Select(MapToDto).ToList();
        }

        public async Task<FileResponseModel> UploadFileAsync(IFormFile file, FileUploadModel uploadDto)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            
            var fileEntity = new FileEntity
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadDate = DateTime.Now,
                ExpiryDate = uploadDto.ExpiryDate,
                Password = uploadDto.Password,
                IsPublic = uploadDto.IsPublic,
                UserId = user?.Id,
                SenderEmail = uploadDto.SenderEmail,
                RecipientEmail = uploadDto.RecipientEmail,
                Message = uploadDto.Message
            };

            var filePath = Path.Combine(_uploadPath, fileEntity.Id.ToString());
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _context.Files.Add(fileEntity);
            await _context.SaveChangesAsync();

            return MapToDto(fileEntity);
        }

        public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(Guid fileId, string? password = null)
        {
            var file = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == fileId)
                ?? throw new FileNotFoundException($"File with ID {fileId} not found.");

            if (!string.IsNullOrEmpty(file.Password) && file.Password != password)
            {
                throw new UnauthorizedAccessException("Invalid password.");
            }

            if (file.ExpiryDate.HasValue && file.ExpiryDate.Value < DateTime.Now)
            {
                throw new InvalidOperationException("File has expired.");
            }

            var filePath = Path.Combine(_uploadPath, file.Id.ToString());
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {file.FileName} not found on disk.");
            }

            return (File.OpenRead(filePath), file.ContentType, file.FileName);
        }

        public async Task<FileResponseModel> GetFileByIdAsync(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
                return null;

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            
            // Dosya sahibi veya herkese açık dosyalar görüntülenebilir
            if (!file.IsPublic && file.UserId != user?.Id)
                return null;

            return MapToDto(file);
        }

        public async Task<List<FileResponseModel>> GetUserFilesAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user == null)
                return new List<FileResponseModel>();

            var files = await _context.Files
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            return files.Select(MapToDto).ToList();
        }

        public async Task<bool> DeleteFileAsync(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
                return false;

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            
            // Sadece dosya sahibi silebilir
            if (file.UserId != user?.Id)
                return false;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", file.Id.ToString());
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return true;
        }

        private FileResponseModel MapToDto(FileEntity entity)
        {
            return new FileResponseModel
            {
                Id = entity.Id,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                FileSize = entity.FileSize,
                UploadDate = entity.UploadDate,
                ExpiryDate = entity.ExpiryDate,
                HasPassword = !string.IsNullOrEmpty(entity.Password),
                IsPublic = entity.IsPublic,
                UserId = entity.UserId
            };
        }
    }
} 