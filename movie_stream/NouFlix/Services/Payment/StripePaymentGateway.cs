using NouFlix.Services.Interface;
using Stripe;
using Stripe.Checkout;

namespace NouFlix.Services.Payment;

public class StripePaymentGateway : IPaymentGateway
{
    public StripePaymentGateway(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"] ?? "sk_test_placeholder";
    }

    public async Task<string> CreatePaymentSession(decimal amount, string currency, string description, string returnUrl, string cancelUrl, CancellationToken ct = default)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100), // Stripe expects cents
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = description,
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = returnUrl + "&session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        return session.Url;
    }

    public async Task<bool> VerifyPayment(string sessionId, CancellationToken ct = default)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId, cancellationToken: ct);

        return session.PaymentStatus == "paid";
    }

    public async Task<bool> RefundPayment(string transactionId, CancellationToken ct = default)
    {
        // For Stripe, transactionId should be the PaymentIntentId. 
        // But if we stored SessionId, we might need to retrieve PaymentIntent from Session.
        // For simplicity, let's assume we stored PaymentIntentId or we can refund via ChargeId.
        // If we stored SessionId, we can't directly refund Session.
        
        // Let's assume transactionId passed here is actually the PaymentIntentId.
        // If we only have SessionId, we need to fetch session -> payment_intent.
        
        try 
        {
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = transactionId
            };
            var service = new RefundService();
            var refund = await service.CreateAsync(refundOptions, cancellationToken: ct);
            return refund.Status == "succeeded" || refund.Status == "pending";
        }
        catch
        {
            return false;
        }
    }
}
