using System.Text.Json.Serialization;

namespace NouFlix.DTOs;

public class PaymentDto
{
    public class MomoSettings
    {
        public string PartnerCode { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string StatusEndpoint { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public string NotifyUrl { get; set; } = null!;
    }
    
    public class MomoQuery
    {
        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("localMessage")] 
        public string? LocalMessage { get; set; }
    }
    
    public record MomoRes(
        int ResultCode,
        string Message,
        string? OrderId,
        string RequestId,
        string PayUrl,
        string Deeplink,
        string QrCodeUrl,
        string? Status,
        string? LocalMessage
    );
}