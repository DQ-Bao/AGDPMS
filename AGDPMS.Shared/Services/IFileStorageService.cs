using Microsoft.AspNetCore.Components.Forms;

namespace AGDPMS.Shared.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        IBrowserFile file,
        string subFolder,
        string[] allowedExtensions,
        long maxSizeBytes = 100 * 1024 * 1024);

}