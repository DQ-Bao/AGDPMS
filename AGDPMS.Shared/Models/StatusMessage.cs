namespace AGDPMS.Shared.Models;

public sealed class StatusMessage
{
    public required StatusMessageType Type { get; set; }
    public required string Message { get; set; }
    public static StatusMessage Info(string? message) => new() { Type = StatusMessageType.Info, Message = message ?? "Unexpected Error" };
    public static StatusMessage Success(string? message) => new() { Type = StatusMessageType.Success, Message = message ?? "Unexpected Error" };
    public static StatusMessage Warn(string? message) => new() { Type = StatusMessageType.Warn, Message = message ?? "Unexpected Error" };
    public static StatusMessage Error(string? message) => new() { Type = StatusMessageType.Error, Message = message ?? "Unexpected Error" };
    public override string ToString() => Message;
}

public enum StatusMessageType { Info, Success, Warn, Error }