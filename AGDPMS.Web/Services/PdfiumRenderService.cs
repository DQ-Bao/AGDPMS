using PDFtoImage;
using SkiaSharp;

namespace AGDPMS.Web.Services;

public class PdfToImageResult
{
    public string Id { get; set; } = string.Empty;
    public List<string> PageImageUrls { get; set; } = [];
}

public interface IPdfToImageService
{
    Task<PdfToImageResult> ConvertPdfToPngImagesAsync(Stream pdfStream, string? fileName = null, CancellationToken cancellationToken = default);
    Task CleanupAsync(string id);
}

public class PdfToImageService(IWebHostEnvironment env, ILogger<PdfToImageService> logger) : IPdfToImageService
{
    private readonly IWebHostEnvironment _env = env;
    private readonly ILogger<PdfToImageService> _logger = logger;

    public async Task<PdfToImageResult> ConvertPdfToPngImagesAsync(Stream pdfStream, string? fileName = null, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("n");
        var destDir = Path.Combine(_env.WebRootPath, "pdfimages", id);
        Directory.CreateDirectory(destDir);

        // convert stream to base64 string (PDFtoImage requires base64 input)
        pdfStream.Seek(0, SeekOrigin.Begin);
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await pdfStream.CopyToAsync(ms, cancellationToken);
            bytes = ms.ToArray();
        }
        string base64Pdf = Convert.ToBase64String(bytes);

        var result = new PdfToImageResult { Id = id };

        try
        {
            var options = new RenderOptions
            {
                Dpi = 150,
                // optional: Width = 1200, Height = 0, BackgroundColor = SKColors.White
            };

            // convert all pages asynchronously
            int pageIndex = 0;
            await foreach (var bitmap in Conversion.ToImagesAsync(base64Pdf, pages: .., password: null, options, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                pageIndex++;

                var outPath = Path.Combine(destDir, $"{pageIndex}.png");
                using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                using var fs = File.OpenWrite(outPath);
                data.SaveTo(fs);

                result.PageImageUrls.Add($"/pdfimages/{id}/{pageIndex}.png");
                bitmap.Dispose();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF using PDFtoImage");
            try { Directory.Delete(destDir, true); } catch { }
            throw;
        }
    }

    public Task CleanupAsync(string id)
    {
        var dir = Path.Combine(_env.WebRootPath, "pdfimages", id);
        if (Directory.Exists(dir))
        {
            try { Directory.Delete(dir, true); } catch (Exception ex) { _logger.LogWarning(ex, "Cleanup failed for {Dir}", dir); }
        }
        return Task.CompletedTask;
    }
}