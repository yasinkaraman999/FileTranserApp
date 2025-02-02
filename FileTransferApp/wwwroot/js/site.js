// Dosya boyutu kontrolü
function validateFileSize(input) {
    if (input.files && input.files[0]) {
        const maxSize = 104857600; // 100 MB
        if (input.files[0].size > maxSize) {
            alert('Dosya boyutu 100 MB\'dan büyük olamaz.');
            input.value = '';
            return false;
        }
    }
    return true;
}

// Dosya türü kontrolü
function validateFileType(input) {
    if (input.files && input.files[0]) {
        const allowedTypes = [
            'image/jpeg',
            'image/png',
            'application/pdf',
            'application/msword',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
            'application/vnd.ms-excel',
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            'application/zip',
            'application/x-rar-compressed'
        ];
        
        if (!allowedTypes.includes(input.files[0].type)) {
            alert('Bu dosya türü desteklenmiyor.');
            input.value = '';
            return false;
        }
    }
    return true;
}

// Upload işlemleri için gerekli elementler
const dropZone = document.getElementById('dropZone');
const fileInput = document.getElementById('file');
const selectedFile = document.querySelector('.selected-file');
const form = document.getElementById('uploadForm');
const progress = document.querySelector('.progress');
const progressBar = document.querySelector('.progress-bar');
const uploadStatus = document.querySelector('.upload-status');
const percentage = document.querySelector('.percentage');
const status = document.querySelector('.status');

// Drag & Drop olayları
['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    dropZone?.addEventListener(eventName, preventDefaults, false);
    document.body.addEventListener(eventName, preventDefaults, false);
});

['dragenter', 'dragover'].forEach(eventName => {
    dropZone?.addEventListener(eventName, highlight, false);
});

['dragleave', 'drop'].forEach(eventName => {
    dropZone?.addEventListener(eventName, unhighlight, false);
});

dropZone?.addEventListener('drop', handleDrop, false);
fileInput?.addEventListener('change', updateFileName);

// Yardımcı fonksiyonlar
function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

function highlight(e) {
    dropZone.classList.add('dragover');
}

function unhighlight(e) {
    dropZone.classList.remove('dragover');
}

function handleDrop(e) {
    const dt = e.dataTransfer;
    const files = dt.files;
    fileInput.files = files;
    updateFileName();
}

function updateFileName() {
    if (fileInput.files.length > 0) {
        const file = fileInput.files[0];
        selectedFile.textContent = `Seçilen dosya: ${file.name}`;
        selectedFile.style.display = 'block';
    } else {
        selectedFile.style.display = 'none';
    }
}

// Form gönderimi
form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const formData = new FormData(form);
    
    // UI elementlerini sıfırla ve göster
    progress.style.display = 'block';
    uploadStatus.style.display = 'block';
    progressBar.style.width = '0%';
    percentage.textContent = '0%';
    status.textContent = 'Yükleniyor...';
    
    try {
        const xhr = new XMLHttpRequest();
        xhr.open('POST', form.action, true);
        
        // Upload progress
        xhr.upload.onprogress = (e) => {
            if (e.lengthComputable) {
                const percentComplete = Math.round((e.loaded / e.total) * 100);
                progressBar.style.width = percentComplete + '%';
                percentage.textContent = percentComplete + '%';
            }
        };
        
        // Upload tamamlandığında
        xhr.onload = function() {
            if (xhr.status === 200) {
                const response = JSON.parse(xhr.responseText);
                if (response.success) {
                    status.textContent = response.message;
                    setTimeout(() => {
                        window.location.href = `/Files/Details/${response.id}`;
                    }, 1000);
                } else {
                    throw new Error(response.error || 'Yükleme başarısız oldu');
                }
            } else {
                const response = JSON.parse(xhr.responseText);
                throw new Error(response.error || 'Yükleme başarısız oldu');
            }
        };
        
        // Hata durumunda
        xhr.onerror = function() {
            throw new Error('Bağlantı hatası oluştu');
        };
        
        xhr.send(formData);
    } catch (error) {
        status.textContent = 'Hata: ' + error.message;
        progress.style.display = 'none';
    }
});

// Şifre görünürlüğünü değiştir
function togglePassword() {
    const input = document.getElementById('password');
    const icon = document.querySelector('.password-toggle-btn i');
    
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('bi-eye');
        icon.classList.add('bi-eye-slash');
    } else {
        input.type = 'password';
        icon.classList.remove('bi-eye-slash');
        icon.classList.add('bi-eye');
    }
}

// Dosya indirme işlemi
function downloadFile(fileId) {
    fetch(`/Files/Download/${fileId}`)
        .then(response => {
            if (response.ok) {
                return response.blob();
            } else {
                return response.json();
            }
        })
        .then(data => {
            if (data instanceof Blob) {
                const url = window.URL.createObjectURL(data);
                const a = document.createElement('a');
                a.href = url;
                a.download = ''; // Sunucudan gelen dosya adını kullanır
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
            } else if (data.requirePassword) {
                showPasswordModal(fileId);
            } else if (data.error) {
                showError(data.error);
            }
        })
        .catch(error => {
            showError('Dosya indirilirken bir hata oluştu.');
        });
}

// Şifre modalını göster
function showPasswordModal(fileId) {
    const modal = new bootstrap.Modal(document.getElementById('passwordModal'));
    document.getElementById('fileId').value = fileId;
    document.getElementById('password').value = '';
    document.querySelector('.password-error').classList.add('d-none');
    document.querySelector('.download-loading').classList.add('d-none');
    document.querySelector('.password-input-group').classList.remove('d-none');
    document.querySelector('.modal-footer').classList.remove('d-none');
    modal.show();
}

// Şifreli indirme
function downloadWithPassword() {
    const fileId = document.getElementById('fileId').value;
    const password = document.getElementById('password').value;
    const errorElement = document.querySelector('.password-error');
    const loadingElement = document.querySelector('.download-loading');
    const inputGroup = document.querySelector('.password-input-group');
    const modalFooter = document.querySelector('.modal-footer');

    if (!password) {
        errorElement.textContent = 'Lütfen şifre giriniz.';
        errorElement.classList.remove('d-none');
        return;
    }

    // Loading göster
    loadingElement.classList.remove('d-none');
    inputGroup.classList.add('d-none');
    modalFooter.classList.add('d-none');
    errorElement.classList.add('d-none');

    const data = {
        id: fileId,
        password: password
    };

    fetch('/Files/DownloadWithPassword', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(data)
    })
    .then(response => {
        if (response.ok) {
            return response.blob();
        } else {
            return response.json().then(data => {
                throw new Error(data.error || 'Dosya indirilirken bir hata oluştu.');
            });
        }
    })
    .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = ''; // Sunucudan gelen dosya adını kullanır
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);

        // Modal'ı kapat
        setTimeout(() => {
            bootstrap.Modal.getInstance(document.getElementById('passwordModal')).hide();
        }, 1000);
    })
    .catch(error => {
        // Hata durumunda loading'i gizle ve hata mesajını göster
        loadingElement.classList.add('d-none');
        inputGroup.classList.remove('d-none');
        modalFooter.classList.remove('d-none');
        errorElement.textContent = error.message;
        errorElement.classList.remove('d-none');
    });
}

// Hata gösterme
function showError(message) {
    const errorElement = document.querySelector('.password-error');
    errorElement.textContent = message;
    errorElement.classList.remove('d-none');
}

// Enter tuşu ile şifreli indirme
document.addEventListener('DOMContentLoaded', function() {
    const passwordInput = document.getElementById('password');
    if (passwordInput) {
        passwordInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                downloadWithPassword();
            }
        });
    }
}); 