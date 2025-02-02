using Microsoft.AspNetCore.Mvc;
using FileTransferApp.Interfaces;
using FileTransferApp.Models;
using FileTransferApp.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace FileTransferApp.Controllers
{
    public class FilesController : Controller
    {
        private readonly Interfaces.IFileService _fileService;
        private readonly IEmailService _emailService;

        public FilesController(Interfaces.IFileService fileService, IEmailService emailService)
        {
            _fileService = fileService;
            _emailService = emailService;
        }

        // GET: Files
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.GetAllFilesAsync();
            return View(files);
        }

      

        // POST: Files/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Model geçerli değil", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                if (model.File == null)
                {
                    return BadRequest(new { error = "Lütfen bir dosya seçin." });
                }

                // Varsayılan son kullanma tarihini ayarla
                model.SetDefaultExpiryDate();

                var uploadDto = new FileUploadModel
                {
                    ExpiryDate = model.ExpiryDate,
                    Password = model.Password,
                    IsPublic = true,
                    SenderEmail = model.SenderEmail,
                    RecipientEmail = model.RecipientEmail,
                    Message = model.Message
                };

                var result = await _fileService.UploadFileAsync(model.File, uploadDto);

                // Mail gönderimi
                if (!string.IsNullOrEmpty(model.RecipientEmail))
                {
                    var downloadLink = Url.Action("Download", "Files", 
                        new { id = result.Id }, Request.Scheme);
                    
                    await _emailService.SendFileDownloadEmailAsync(
                        model.RecipientEmail,
                        model.SenderEmail ?? "Anonim",
                        model.Message,
                        downloadLink
                    );
                }

                return Json(new { 
                    success = true,
                    id = result.Id,
                    fileName = result.FileName,
                    fileSize = result.FileSize,
                    uploadDate = result.UploadDate,
                    message = "Dosya başarıyla yüklendi."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Dosya yüklenirken bir hata oluştu: {ex.Message}" });
            }
        }

        // GET: Files/Download/5
        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var fileInfo = await _fileService.GetFileInfoAsync(id);
                var result = await _fileService.DownloadFileAsync(id, null);
                return File(result.FileStream, result.ContentType, result.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { requirePassword = true });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "Dosya bulunamadı." });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "Dosyanın süresi dolmuş." });
            }
        }

        // POST: Files/DownloadWithPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadWithPassword([FromBody] DownloadWithPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Lütfen şifre giriniz." });
                }

                var result = await _fileService.DownloadFileAsync(request.Id, request.Password);
                return File(result.FileStream, result.ContentType, result.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                return BadRequest(new { error = "Geçersiz şifre." });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "Dosya bulunamadı." });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "Dosyanın süresi dolmuş." });
            }
        }

        // POST: Files/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _fileService.DeleteFileAsync(id);
                TempData["SuccessMessage"] = "Dosya başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Dosya silinirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Files/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var file = await _fileService.GetFileInfoAsync(id);
                return View(file);
            }
            catch (FileNotFoundException)
            {
                TempData["ErrorMessage"] = "Dosya bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Files/download-temp/{id}
        [Route("Files/download-temp/{id}")]
        public async Task<IActionResult> DownloadTemp(Guid id)
        {
            try
            {
                var file = await _fileService.GetFileInfoAsync(id);
                if (file == null || file.IsExpired)
                {
                    return View("FileNotFound");
                }
                return View("DownloadTemp", file);
            }
            catch (FileNotFoundException)
            {
                return View("FileNotFound");
            }
        }
    }

    public class DownloadWithPasswordRequest
    {
        public Guid Id { get; set; }
        public string Password { get; set; }
    }
} 