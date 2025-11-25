using NouFlix.Services.Interface;

namespace NouFlix.Services.Payment;



public class PaymentGatewayFactory(IServiceProvider serviceProvider)
{
    public IPaymentGateway Create(string provider)
    {
        return provider.ToLower() switch
        {
            "momo" => serviceProvider.GetRequiredService<MomoPaymentGateway>(),
            "stripe" => serviceProvider.GetRequiredService<StripePaymentGateway>(),
            _ => throw new ArgumentException($"Payment provider '{provider}' not supported")
        };
    }
}
