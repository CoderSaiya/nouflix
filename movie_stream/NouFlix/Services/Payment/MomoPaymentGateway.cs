using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NouFlix.DTOs;
using NouFlix.Services.Interface;

namespace NouFlix.Services.Payment;

public class MomoPaymentGateway(
    IOptions<PaymentDto.MomoSettings> momoOptions,
    HttpClient httpClient)
    : IPaymentGateway
{
    private readonly PaymentDto.MomoSettings _cfg = momoOptions.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string RequestTypeCreate = "captureWallet";
    private const string RequestTypeStatus = "transactionStatus";

    public async Task<string> CreatePaymentSession(
        decimal amount,
        string currency,
        string description,
        string returnUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        var orderId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();
        
        var amountStr = ((long)amount).ToString();
        
        var raw = BuildQueryString(new()
        {
            { "accessKey", _cfg.AccessKey },
            { "amount", amountStr },
            { "extraData", "" },
            { "ipnUrl", _cfg.NotifyUrl },
            { "orderId", orderId },
            { "orderInfo", description },
            { "partnerCode", _cfg.PartnerCode },
            { "redirectUrl", returnUrl },
            { "requestId", requestId },
            { "requestType", RequestTypeCreate }
        });

        var signature = HmacSign(raw);

        var payload = new
        {
            partnerCode = _cfg.PartnerCode,
            partnerName = "Nouflix",
            storeId = "MomoTestStore",
            requestId,
            amount = (long)amount,
            orderId,
            orderInfo = description,
            redirectUrl = returnUrl,
            ipnUrl = _cfg.NotifyUrl,
            requestType = RequestTypeCreate,
            lang = "vi",
            extraData = "",
            signature
        };

        var resp = await httpClient.PostAsJsonAsync(_cfg.Endpoint, payload, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"MoMo API Failed ({resp.StatusCode}): {body}");

        var momo = JsonSerializer.Deserialize<PaymentDto.MomoRes>(body, JsonOptions)
                   ?? throw new Exception("Invalid MoMo JSON.");

        if (momo.ResultCode != 0)
            throw new Exception($"MoMo Error {momo.ResultCode}: {momo.Message}");

        return string.IsNullOrEmpty(momo.PayUrl) ? momo.QrCodeUrl : momo.PayUrl;
    }
    
    public async Task<bool> VerifyPayment(string orderId, CancellationToken ct = default)
    {
        var requestId = Guid.NewGuid().ToString();

        var raw = BuildQueryString(new()
        {
            { "accessKey", _cfg.AccessKey },
            { "orderId", orderId },
            { "partnerCode", _cfg.PartnerCode },
            { "requestId", requestId }
        });

        var signature = HmacSign(raw);

        var payload = new
        {
            partnerCode = _cfg.PartnerCode,
            accessKey = _cfg.AccessKey,
            requestId,
            orderId,
            signature
        };

        var resp = await httpClient.PostAsJsonAsync(_cfg.StatusEndpoint, payload, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"MoMo Status Error ({resp.StatusCode}): {body}");

        var momo = JsonSerializer.Deserialize<PaymentDto.MomoQuery>(body, JsonOptions)
                   ?? throw new Exception("Invalid JSON");

        return momo.ResultCode == 0;
    }
    
    public Task<bool> RefundPayment(string transactionId, CancellationToken ct = default)
        => Task.FromResult(true);
    
    private static string BuildQueryString(Dictionary<string, string> dict)
        => string.Join("&", dict.OrderBy(k => k.Key).Select(k => $"{k.Key}={k.Value}"));

    private string HmacSign(string rawData)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_cfg.SecretKey));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData)))
            .ToLowerInvariant();
    }
}