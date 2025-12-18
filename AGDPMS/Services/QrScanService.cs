namespace AGDPMS.Services;

public class QrScanService
{
    public event Action<string>? QrCodeScanned;
    public void Publish(string value)
    {
        QrCodeScanned?.Invoke(value);
    }
}