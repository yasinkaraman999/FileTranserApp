using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace FileTransferApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public virtual ICollection<FileEntity> Files { get; set; }
    }
} 