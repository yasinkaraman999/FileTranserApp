# .NET Core ile Modern Bir Dosya Transfer Uygulaması

Bu makalede, .NET Core kullanarak geliştirdiğimiz modern ve güvenli dosya transfer uygulamasını sizlerle paylaşacağız.

## Giriş

Günümüzde dosya paylaşımı, günlük iş akışımızın vazgeçilmez bir parçası haline geldi. WeTransfer gibi platformlar bu ihtiyacı karşılasa da, kendi sunucularımızda çalışan, özelleştirilebilir ve güvenli bir çözüme ihtiyaç duyduk. Bu nedenle FileTransferApp projesini geliştirmeye karar verdik.

## Temel Özellikler

### Güvenli Dosya Transferi
- Dosyalar şifre korumalı olarak paylaşılabilir
- Her dosya için benzersiz ve tahmin edilemez URL'ler oluşturulur
- Dosyalara doğrudan URL ile erişim engellendi
- Yalnızca yetkili kullanıcılar dosyaları görüntüleyebilir

### Akıllı Dosya Yönetimi
- Otomatik dosya süresi sona erme sistemi
- Dosya boyutu ve türü filtreleme
- Şifreli dosya paylaşımı
- Dosya durum takibi

### Bildirim Sistemi
- Otomatik e-posta bildirimleri
- Dosya paylaşım bilgilendirmeleri
- Süre sonu yaklaşan dosyalar için uyarılar

## Teknik Detaylar

### Veritabanı Yapısı

Dosya yönetimi için tasarlanan veritabanı yapısında şu önemli alanlar bulunuyor:
- Benzersiz dosya kimliği (GUID formatında)
- Dosya meta bilgileri (ad, boyut, tür)
- Güvenlik bilgileri (şifre, erişim kontrolü)
- Zaman damgaları (yükleme tarihi, son kullanma tarihi)
- İlişkisel kullanıcı bağlantıları
- E-posta ve mesaj bilgileri

### Güvenlik Katmanı

Uygulama güvenliği çok katmanlı bir yapıda tasarlandı:

1. Kullanıcı Kimlik Doğrulama
```csharp
[Authorize]
public class FilesController : Controller
{
    // Tüm dosya işlemleri kimlik doğrulama gerektirir
}
```

2. Dosya Erişim Kontrolü
```csharp
// Şifreli dosya kontrolü
if (!string.IsNullOrEmpty(file.Password) && file.Password != password)
{
    throw new UnauthorizedAccessException("Geçersiz şifre.");
}

// Süre kontrolü
if (file.ExpiryDate.HasValue && file.ExpiryDate.Value < DateTime.Now)
{
    throw new InvalidOperationException("Dosyanın süresi dolmuş.");
}
```

3. URL Güvenliği
- Her dosya için benzersiz GUID oluşturulur
- URL'ler tahmin edilemez yapıdadır
- Doğrudan dosya erişimi engellenir

### Dosya İşleme Sistemi

Dosya yükleme ve işleme süreçleri şu adımlarla gerçekleşir:

1. Dosya Doğrulama
- Boyut kontrolü (maksimum 100MB)
- Dosya türü kontrolü (izin verilen formatlar)
- Model doğrulama kontrolleri
- Zorunlu alan kontrolleri

2. Güvenli Depolama
- Dosyalar benzersiz isimlerle saklanır
- Fiziksel dosya yolu gizlenir
- Yedekleme sistemi

3. Erişim Yönetimi
- Şifre koruması
- Süre sınırlaması
- Kullanıcı bazlı erişim kontrolü

İşte dosya yükleme işleminin nasıl gerçekleştiğini gösteren örnek kod:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Upload(FileUploadViewModel model)
{
    // Model doğrulama kontrolü
    if (!ModelState.IsValid)
    {
        return BadRequest(new { error = "Model geçerli değil" });
    }

    try
    {
        // Dosya kontrolü
        if (model.File == null)
        {
            return BadRequest(new { error = "Lütfen bir dosya seçin." });
        }

        // Son kullanma tarihi ayarlanır
        model.SetDefaultExpiryDate();

        // Upload DTO hazırlanır
        var uploadDto = new FileUploadModel
        {
            ExpiryDate = model.ExpiryDate,
            Password = model.Password,
            IsPublic = true,
            SenderEmail = model.SenderEmail,
            RecipientEmail = model.RecipientEmail,
            Message = model.Message
        };

        // Dosya yükleme işlemi gerçekleştirilir
        var result = await _fileService.UploadFileAsync(model.File, uploadDto);

        // E-posta bildirimi gönderilir
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

        // Başarılı sonuç döndürülür
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
```

Bu kod parçası, dosya yükleme işleminin tüm aşamalarını göstermektedir:
1. CSRF koruması için ValidateAntiForgeryToken kullanılır
2. Model doğrulaması yapılır
3. Dosya varlığı kontrol edilir
4. Son kullanma tarihi ayarlanır
5. Dosya yükleme işlemi gerçekleştirilir
6. Alıcıya e-posta bildirimi gönderilir
7. İşlem sonucu JSON formatında döndürülür

Tüm bu süreç güvenli bir şekilde gerçekleştirilir ve her aşamada olası hatalar yakalanıp kullanıcıya bildirilir.

### Bildirim Sistemi

E-posta bildirimleri HTML şablonları kullanılarak özelleştirildi:
- Dosya paylaşım bildirimleri
- Güvenli indirme linkleri
- Özelleştirilmiş mesajlar

## Sonuç

Bu proje ile güvenli ve kullanıcı dostu bir dosya transfer sistemi geliştirildi. Özellikle:

- Çok katmanlı güvenlik yapısı
- Akıllı dosya yönetimi
- Otomatik bildirim sistemi
- Kullanıcı dostu arayüz

gibi özelliklerle kurumsal ihtiyaçlar karşılandı.

### Gelecek Planları

Projeye eklenecek bazı özellikler:

- AWS S3 entegrasyonu
- Dosya önizleme sistemi
- Klasör paylaşımı
- Mobil uygulama
- Genişletilmiş API desteği

Bu özellikler yakın zamanda projeye dahil edilecektir.