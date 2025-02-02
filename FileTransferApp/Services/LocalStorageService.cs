using FileTransferApp.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FileTransferApp.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _storagePath;
        private readonly IWebHostEnvironment _environment;

        public LocalStorageService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            _storagePath = configuration.GetValue<string>("FileSettings:LocalStoragePath") ?? "wwwroot/uploads";
            
            // Ensure storage directory exists
            var fullPath = Path.Combine(_environment.WebRootPath, _storagePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, _storagePath, fileName);
            
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(_storagePath, fileName);
        }

        public Task<Stream> DownloadFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
        }

        public Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        public bool FileExists(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            return File.Exists(fullPath);
        }
    }
} 