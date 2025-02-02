using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FileTransferApp.Models
{
    public class FileUploadViewModel
    {
        [Required(ErrorMessage = "Lütfen bir dosya seçin.")]
        [Display(Name = "Dosya")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "Alıcı e-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "Alıcı E-posta")]
        public string RecipientEmail { get; set; }

        [Required(ErrorMessage = "Gönderen e-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "Gönderen E-posta")]
        public string SenderEmail { get; set; }

        [Display(Name = "Mesaj")]
        public string Message { get; set; }

        [Display(Name = "Şifre")]
        public string? Password { get; set; }

        [Display(Name = "Son Kullanma Tarihi")]
        public DateTime? ExpiryDate { get; set; }

        public void SetDefaultExpiryDate()
        {
            if (!ExpiryDate.HasValue)
            {
                ExpiryDate = DateTime.Now.AddDays(7);
            }
        }
    }
} 