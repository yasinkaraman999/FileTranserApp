using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileTransferApp.Models;
using Microsoft.AspNetCore.Http;

namespace FileTransferApp.Interfaces
{
    public interface IFileService
    {
        Task<FileResponseModel> UploadFileAsync(IFormFile file, string? password = null, DateTime? expiryDate = null);
        Task<FileResponseModel> UploadFileAsync(IFormFile file, FileUploadModel uploadDto);
        Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(Guid fileId, string? password = null);
        Task<FileResponseModel> GetFileByIdAsync(Guid id);
        Task<FileResponseModel> GetFileInfoAsync(Guid id);
        Task<List<FileResponseModel>> GetUserFilesAsync();
        Task<List<FileResponseModel>> GetAllFilesAsync(bool includeDeleted = false);
        Task<bool> DeleteFileAsync(Guid id);
    }
} 