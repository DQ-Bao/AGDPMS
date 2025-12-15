using AGDPMS.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;

namespace AGDPMS.Web.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _uploadsRootFolder;

    public FileStorageService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        
        // Ensure WebRootPath is set
        if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
        {
            throw new InvalidOperationException("WebRootPath is not set. Cannot save files.");
        }
        
        _uploadsRootFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        
        // Ensure the uploads root folder exists
        if (!Directory.Exists(_uploadsRootFolder))
        {
            Directory.CreateDirectory(_uploadsRootFolder);
        }
    }

    public async Task<string> SaveFileAsync(
        IBrowserFile file,
        string subFolder,
        string[] allowedExtensions,
        long maxSizeBytes = 100 * 1024 * 1024)
    {
        if (file.Size > maxSizeBytes)
        {
            long maxMb = maxSizeBytes / 1024 / 1024;
            throw new Exception($"Lỗi: File '{file.Name}' vượt quá giới hạn {maxMb}MB.");
        }

        var fileExtension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            throw new Exception($"Lỗi: Định dạng file '{fileExtension}' không được phép. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
        }

        var targetFolder = Path.Combine(_uploadsRootFolder, subFolder);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // Preserve original filename but make it unique to avoid conflicts
        var originalFileName = Path.GetFileNameWithoutExtension(file.Name);
        // Sanitize filename: remove invalid characters
        var sanitizedFileName = string.Join("_", originalFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        // If sanitization removed everything, use a default name
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            sanitizedFileName = "file";
        }
        // Limit filename length to avoid path too long errors
        if (sanitizedFileName.Length > 100)
        {
            sanitizedFileName = sanitizedFileName.Substring(0, 100);
        }
        
        // Create unique filename: original_name_GUID.extension
        var uniqueFileName = $"{sanitizedFileName}_{Guid.NewGuid():N}{fileExtension}";
        var physicalPath = Path.Combine(targetFolder, uniqueFileName);

        // Sử dụng using statement đúng cách và giới hạn buffer size
        await using var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);

        // Sử dụng CopyToAsync với buffer size cụ thể và đảm bảo stream được dispose
        const int bufferSize = 80 * 1024; // 80KB buffer
        await using var stream = file.OpenReadStream(maxSizeBytes);
        await stream.CopyToAsync(fileStream, bufferSize);
        
        // Explicitly flush and ensure file is written to disk
        await fileStream.FlushAsync();

        // Verify the file was actually created
        if (!File.Exists(physicalPath))
        {
            throw new Exception($"Lỗi: Không thể lưu file '{file.Name}'. File không tồn tại sau khi ghi.");
        }

        // Verify file size matches
        var savedFileInfo = new FileInfo(physicalPath);
        if (savedFileInfo.Length == 0)
        {
            throw new Exception($"Lỗi: File '{file.Name}' được lưu nhưng có kích thước 0 bytes.");
        }

        var webPath = $"/uploads/{subFolder}/{uniqueFileName}";
        return webPath;
    }

    public Task<bool> DeleteFileAsync(string webPath)
    {
        try
        {
            if (string.IsNullOrEmpty(webPath) || !webPath.StartsWith("/uploads/"))
            {
                return Task.FromResult(false);
            }

            var physicalPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}