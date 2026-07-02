namespace BLL.DTOs.Contracts
{
    public class PaymentResponseDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
