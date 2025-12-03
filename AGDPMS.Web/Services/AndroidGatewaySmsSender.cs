using Microsoft.Extensions.Options;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using AGDPMS.Shared.Services;

namespace AGDPMS.Web.Services;

public class AndroidSmsGatewayOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AndroidGatewaySmsSender : ISmsSender
{
    private readonly AndroidSmsGatewayOptions _opts;
    private readonly HttpClient _http;
    
    public AndroidGatewaySmsSender(IOptions<AndroidSmsGatewayOptions> options, IHttpClientFactory httpClientFactory)
    {
        _opts = options.Value;
        _http = httpClientFactory.CreateClient();
        _http.BaseAddress = new Uri("https://api.sms-gate.app/3rdparty/v1/");
        var authBytes = Encoding.UTF8.GetBytes($"{_opts.Username}:{_opts.Password}");
        var authHeader = Convert.ToBase64String(authBytes);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
    }

    public async Task SendAsync(string message, string[] phoneNumbers)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message can not be empty", nameof(message));
        if (phoneNumbers == null || phoneNumbers.Length == 0)
            throw new ArgumentException("Require at least one phone number", nameof(phoneNumbers));
        var json = JsonSerializer.Serialize(new
        {
            textMessage = new { text = message },
            phoneNumbers = phoneNumbers.Select(p => NormalizePhoneNumber(p, "84")).ToArray()
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync("messages", content);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to send SMS. Status {resp.StatusCode}: {body}");
        }
    }

    private static string NormalizePhoneNumber(string phone, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number can't be empty", nameof(phone));
        phone = new string([.. phone.Trim().Where(char.IsDigit)]);
        if (phone.StartsWith(countryCode))
            return '+' + phone;
        if (phone.StartsWith('0'))
            phone = phone[1..];
        return '+' + countryCode + phone;
    }
}
