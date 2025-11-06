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
        _uploadsRootFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
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

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var physicalPath = Path.Combine(targetFolder, uniqueFileName);

        // Sử dụng using statement đúng cách và giới hạn buffer size
        await using var fileStream = new FileStream(physicalPath, FileMode.Create);

        // Sử dụng CopyToAsync với buffer size cụ thể
        const int bufferSize = 80 * 1024; // 80KB buffer
        var stream = file.OpenReadStream(maxSizeBytes);
        await stream.CopyToAsync(fileStream, bufferSize);

        var webPath = $"/uploads/{subFolder}/{uniqueFileName}";
        return webPath;
    }
}