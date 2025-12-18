using AGDPMS.Services;
using ZXing.Net.Maui;

namespace AGDPMS;

public partial class QrScannerPage : ContentPage
{
    private readonly QrScanService _service;
	private bool _scanned = false;

    public QrScannerPage(QrScanService service)
	{
		InitializeComponent();
        _service = service;
    }

    private async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_scanned) return;
        var value = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(value)) return;
        _scanned = true;
        _service.Publish(value);
        await MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync("//main"));
    }
}