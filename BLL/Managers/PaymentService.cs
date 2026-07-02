using BLL.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLL.Managers
{
    /// <summary>
    /// Simulated payment service for development.
    /// Replace with real Paymob integration in production.
    /// </summary>
    public class SimulatedPaymentService : IPaymentService
    {
        private readonly ILogger<SimulatedPaymentService> _logger;

        public SimulatedPaymentService(ILogger<SimulatedPaymentService> logger)
        {
            _logger = logger;
        }

        public async Task<string> InitiatePaymentAsync(int orderId, decimal amount, string buyerEmail, string buyerName)
        {
            _logger.LogInformation(
                "Simulated payment for order {OrderId}: {Amount} EGP from {Email}",
                orderId, amount, buyerEmail);

            // Simulate network delay
            await Task.Delay(500);

            // Return a fake success URL
            return $"/api/payments/simulated-callback?orderId={orderId}&amount={amount}&status=completed";
        }
    }

    /// <summary>
    /// Real Paymob payment gateway integration for production.
    /// </summary>
    public class PaymobPaymentService : IPaymentService
    {
        private readonly PaymobSettings _settings;
        private readonly ILogger<PaymobPaymentService> _logger;

        public PaymobPaymentService(IOptions<PaymobSettings> settings, ILogger<PaymobPaymentService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> InitiatePaymentAsync(int orderId, decimal amount, string buyerEmail, string buyerName)
        {
            // TODO: Implement real Paymob API integration
            // 1. Get auth token from Paymob
            // 2. Create order in Paymob
            // 3. Get payment key
            // 4. Generate iframe URL

            _logger.LogInformation(
                "Paymob payment initiated for order {OrderId}: {Amount} EGP",
                orderId, amount);

            await Task.CompletedTask;

            // Placeholder URL — replace with real Paymob iframe URL
            return $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token=PLACEHOLDER";
        }
    }
}
