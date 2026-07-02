namespace BLL.Managers
{
    public interface IPaymentService
    {
        /// <summary>Initiate a payment and return a redirect URL.</summary>
        Task<string> InitiatePaymentAsync(int orderId, decimal amount, string buyerEmail, string buyerName);
    }
}
