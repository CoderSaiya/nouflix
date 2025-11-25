namespace NouFlix.Services.Interface;

public interface IPaymentGateway
{
    Task<string> CreatePaymentSession(decimal amount, string currency, string description, string returnUrl, string cancelUrl, CancellationToken ct = default);
    Task<bool> VerifyPayment(string sessionId, CancellationToken ct = default);
    Task<bool> RefundPayment(string transactionId, CancellationToken ct = default);
}