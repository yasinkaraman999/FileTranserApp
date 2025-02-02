using FileTransferApp.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileTransferApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileEntity> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // FileEntity konfigürasyonu
            builder.Entity<FileEntity>(entity =>
            {
                entity.ToTable("Files");
                entity.HasKey(e => e.Id);

                // Zorunlu alanlar
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.UploadDate).IsRequired();
                entity.Property(e => e.IsPublic).IsRequired();

                // İsteğe bağlı alanlar
                entity.Property(e => e.Password).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.SenderEmail).HasMaxLength(255).IsRequired(false);
                entity.Property(e => e.RecipientEmail).HasMaxLength(255).IsRequired(false);
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.UserId).IsRequired(false);

                // İlişkiler
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Files)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
            });

            // ApplicationUser konfigürasyonu
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                // İlişkiler
                entity.HasMany(e => e.Files)
                    .WithOne(f => f.User)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
            });
        }
    }
} 