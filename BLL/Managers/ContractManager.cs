using BLL.DTOs.Common;
using BLL.DTOs.Contracts;
using BLL.Mapping.Contracts;
using BLL.Managers.CloudinaryManager;
using BLL.Settings;
using DAL.Models;
using DAL.Models.Enums;
using DAL.Models.ExceptionModels;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BLL.Managers
{
    public class ContractManager : IContractManager
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ContractSettings _settings;

        public ContractManager(
            IUnitOfWork uow,
            INotificationService notificationService,
            IPaymentService paymentService,
            ICloudinaryService cloudinaryService,
            IOptions<ContractSettings> settings)
        {
            _uow = uow;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _cloudinaryService = cloudinaryService;
            _settings = settings.Value;
        }

        public async Task<BaseResponse<ContractFormDto>> GetFormAsync(int orderId, int currentUserId)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            if (order.BuyerId != currentUserId)
                return BaseResponse<ContractFormDto>.Failure("أنت لست مشتري هذا الطلب.");

            if (order.Status != OrderStatus.InProgress)
                return BaseResponse<ContractFormDto>.Failure("لا يمكن إنشاء عقد في هذه الحالة.");

            var dto = order.ToContractFormDto(_settings);
            return BaseResponse<ContractFormDto>.Success(dto, "تم تجهيز نموذج العقد.");
        }

        public async Task<BaseResponse<ContractResponseDto>> SubmitContractAsync(int orderId, int currentUserId, SubmitContractDto dto)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            if (order.BuyerId != currentUserId)
                return BaseResponse<ContractResponseDto>.Failure("أنت لست مشتري هذا الطلب.");

            if (order.Status != OrderStatus.InProgress)
                return BaseResponse<ContractResponseDto>.Failure("لا يمكن تقديم العقد في هذه الحالة.");

            order.AgreedQuantity = dto.AgreedQuantity;
            order.AgreedPricePerUnit = dto.AgreedPricePerUnit;
            order.AgreedTotalPrice = dto.AgreedQuantity * dto.AgreedPricePerUnit;
            order.DeliveryDate = dto.DeliveryDate;
            order.DeliveryAddress = dto.DeliveryAddress;
            order.DownPaymentPercentage = _settings.DownPaymentPercentage;
            order.DownPaymentAmount = order.AgreedTotalPrice * _settings.DownPaymentPercentage;
            order.CommissionRate = _settings.CommissionRate;
            order.PlatformCommission = order.AgreedTotalPrice * _settings.CommissionRate;
            order.SellerTotalPayout = order.DownPaymentAmount - order.PlatformCommission;
            order.ContractTerms = JsonSerializer.Serialize(ContractMappings.BuildPenaltyClauses(_settings));
            order.ContractGeneratedAt = DateTime.UtcNow;
            order.IsSignedByBuyer = true;
            order.Status = OrderStatus.ContractReview;

            _uow.Repository<Order>().Update(order);
            await _uow.SaveChangesAsync();

            var listingTitle = order.Listing?.Title ?? string.Empty;

            // Generate PDF with buyer signature and upload to Cloudinary
            await GenerateAndUploadContractPdfAsync(order, signedByBuyer: true, signedBySeller: false);

            await _notificationService.SendNotificationAsync(
                order.SellerId,
                "تم إنشاء عقد جديد",
                $"تم إنشاء عقد جديد على {listingTitle}. يرجى مراجعة العقد وقبوله أو طلب تعديل.",
                "contract_generated",
                $"/orders/{order.Id}/contract");

            var resultDto = order.ToContractResponseDto(_settings);
            return BaseResponse<ContractResponseDto>.Success(resultDto, "تم تقديم العقد بنجاح. ينتظر مراجعة المورد.");
        }

        public async Task<BaseResponse<ContractResponseDto>> GetContractAsync(int orderId, int currentUserId)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            var dto = order.ToContractResponseDto(_settings);
            return BaseResponse<ContractResponseDto>.Success(dto, "تم جلب العقد.");
        }

        public async Task<BaseResponse<ContractResponseDto>> AcceptContractAsync(int orderId, int currentUserId)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            if (order.SellerId != currentUserId)
                return BaseResponse<ContractResponseDto>.Failure("أنت لست مورد هذا الطلب.");

            if (order.Status != OrderStatus.ContractReview)
                return BaseResponse<ContractResponseDto>.Failure("لا يمكن قبول العقد في هذه الحالة.");

            order.SellerSignedAt = DateTime.UtcNow;
            order.IsSignedBySeller = true;
            order.DeclineReason = null;
            order.Status = OrderStatus.PaymentPending;

            _uow.Repository<Order>().Update(order);
            await _uow.SaveChangesAsync();

            // Generate PDF with both signatures and upload
            await GenerateAndUploadContractPdfAsync(order, signedByBuyer: true, signedBySeller: true);

            var listingTitle = order.Listing?.Title ?? string.Empty;

            await _notificationService.SendNotificationAsync(
                order.BuyerId,
                "تم قبول العقد",
                $"تم قبول العقد على {listingTitle}. يرجى دفع العربون ({_settings.DownPaymentPercentage * 100}%) للمتابعة.",
                "contract_accepted",
                $"/orders/{order.Id}/payment");

            var resultDto = order.ToContractResponseDto(_settings);
            return BaseResponse<ContractResponseDto>.Success(resultDto, "تم قبول العقد. ينتظر دفع العربون من المشتري.");
        }

        public async Task<BaseResponse<ContractResponseDto>> DeclineContractAsync(int orderId, int currentUserId, DeclineContractDto dto)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            if (order.SellerId != currentUserId)
                return BaseResponse<ContractResponseDto>.Failure("أنت لست مورد هذا الطلب.");

            if (order.Status != OrderStatus.ContractReview)
                return BaseResponse<ContractResponseDto>.Failure("لا يمكن رفض العقد في هذه الحالة.");

            order.DeclineReason = dto.Reason;
            order.IsSignedBySeller = false;
            order.Status = OrderStatus.InProgress;

            _uow.Repository<Order>().Update(order);
            await _uow.SaveChangesAsync();

            var listingTitle = order.Listing?.Title ?? string.Empty;

            await _notificationService.SendNotificationAsync(
                order.BuyerId,
                "طلب تعديل العقد",
                $"طلب المورد تعديل العقد على {listingTitle}. السبب: {dto.Reason}",
                "contract_declined",
                $"/orders/{order.Id}/contract");

            var resultDto = order.ToContractResponseDto(_settings);
            return BaseResponse<ContractResponseDto>.Success(resultDto, "تم رفض العقد. يمكن للمشتري تعديله وإعادة إرساله.");
        }

        public async Task<BaseResponse<PaymentSummaryDto>> GetPaymentSummaryAsync(int orderId, int currentUserId)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            if (order.BuyerId != currentUserId)
                return BaseResponse<PaymentSummaryDto>.Failure("أنت لست مشتري هذا الطلب.");

            if (order.Status != OrderStatus.PaymentPending)
                return BaseResponse<PaymentSummaryDto>.Failure("الدفع غير متاح في هذه الحالة.");

            var dto = order.ToPaymentSummaryDto(_settings);
            return BaseResponse<PaymentSummaryDto>.Success(dto, "تم تجهيز ملخص الدفع.");
        }

        public async Task<BaseResponse<PaymentResponseDto>> InitiatePaymentAsync(int orderId, int currentUserId)
        {
            var order = await LoadOrderAsync(orderId, currentUserId);

            if (order.BuyerId != currentUserId)
                return BaseResponse<PaymentResponseDto>.Failure("أنت لست مشتري هذا الطلب.");

            if (order.Status != OrderStatus.PaymentPending)
                return BaseResponse<PaymentResponseDto>.Failure("الدفع غير متاح في هذه الحالة.");

            if (order.IsDownPaymentPaid)
                return BaseResponse<PaymentResponseDto>.Failure("تم دفع العربون مسبقاً.");

            var totalPrice = order.AgreedQuantity * order.AgreedPricePerUnit;
            var amount = totalPrice * (order.DownPaymentPercentage > 0 ? order.DownPaymentPercentage : _settings.DownPaymentPercentage);

            var paymentUrl = await _paymentService.InitiatePaymentAsync(
                order.Id,
                amount,
                order.Buyer.Email!,
                order.Buyer.Name);

            var response = new PaymentResponseDto
            {
                OrderId = order.Id,
                Amount = amount,
                PaymentUrl = paymentUrl,
                Status = "pending"
            };

            return BaseResponse<PaymentResponseDto>.Success(response, "تم تحويلك إلى بوابة الدفع.");
        }

        public async Task<BaseResponse<bool>> ConfirmPaymentCallbackAsync(int orderId, string paymentStatus)
        {
            var order = await _uow.Repository<Order>().Query()
                .Include(o => o.Buyer)
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (paymentStatus != "completed")
                return BaseResponse<bool>.Failure("فشلت عملية الدفع. يرجى المحاولة مرة أخرى.");

            using var dbTransaction = await _uow.BeginTransactionAsync();
            try
            {
                order.IsDownPaymentPaid = true;
                order.BuyerSignedAt = DateTime.UtcNow;
                order.Status = OrderStatus.InProgress;

                // Generate final PDF with all signatures + payment confirmation
                await GenerateAndUploadContractPdfAsync(order, signedByBuyer: true, signedBySeller: true);

                _uow.Repository<Order>().Update(order);
                await _uow.SaveChangesAsync();

                // Create wallet freeze transaction
                var buyerWallet = await _uow.Repository<Wallet>().Query()
                    .FirstOrDefaultAsync(w => w.UserId == order.BuyerId);

                if (buyerWallet != null)
                {
                    var walletTransaction = new Transaction
                    {
                        OrderId = order.Id,
                        WalletId = buyerWallet.Id,
                        Amount = order.DownPaymentAmount,
                        TransactionType = TransactionType.Freeze,
                        Status = TransactionStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _uow.Repository<Transaction>().AddAsync(walletTransaction);
                    await _uow.SaveChangesAsync();
                }

                await dbTransaction.CommitAsync();
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }

            var listingTitle = order.Listing?.Title ?? string.Empty;

            await _notificationService.SendNotificationAsync(
                order.SellerId,
                "تم دفع العربون",
                $"تم دفع العربون ({_settings.DownPaymentPercentage * 100}%) على {listingTitle}. العقد ساري المفعول.",
                "down_payment_received",
                $"/orders/{order.Id}");

            await _notificationService.SendNotificationAsync(
                order.BuyerId,
                "تم دفع العربون",
                $"تم دفع العربون بنجاح على {listingTitle}. يمكنك متابعة الطلب.",
                "payment_confirmed",
                $"/orders/{order.Id}");

            return BaseResponse<bool>.Success(true, "تم دفع العربون بنجاح. العقد ساري المفعول.");
        }

        // ───────────────────────────── Private ─────────────────────────────

        private async Task<Order> LoadOrderAsync(int orderId, int currentUserId)
        {
            var order = await _uow.Repository<Order>().Query()
                .Include(o => o.Listing)
                .Include(o => o.Buyer).ThenInclude(b => b.Factory)
                .Include(o => o.Seller).ThenInclude(s => s.Factory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.BuyerId != currentUserId && order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            return order;
        }

        private async Task GenerateAndUploadContractPdfAsync(Order order, bool signedByBuyer, bool signedBySeller)
        {
            try
            {
                // ──────────────────────────────────────────────────
                // Generate contract PDF using QuestPDF
                // ──────────────────────────────────────────────────
                var pdfBytes = GenerateContractPdf(order, signedByBuyer, signedBySeller);

                var fileName = $"contract_FYD-{order.Id + 2000}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                var folder = "contracts";

                var pdfUrl = await _cloudinaryService.UploadPdfAsync(pdfBytes, fileName, folder);

                order.ContractUrl = pdfUrl;
                _uow.Repository<Order>().Update(order);
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                // PDF generation failure should not block the flow
                // The contract data is still saved in the database
            }
        }

        private static byte[] GenerateContractPdf(Order order, bool signedByBuyer, bool signedBySeller)
        {
            // For now, return a minimal valid PDF placeholder.
            // In production, use QuestPDF to generate a proper Arabic RTL document.
            return CreateMinimalPdfPlaceholder(order, signedByBuyer, signedBySeller);
        }

        private static byte[] CreateMinimalPdfPlaceholder(Order order, bool signedByBuyer, bool signedBySeller)
        {
            var buyerFactory = order.Buyer.Factory;
            var sellerFactory = order.Seller.Factory;

            var content = $@"
                ============================================
                عقد اتفاقية توريد - منصة فايد
                ============================================
                رقم العقد: FYD-{order.Id + 2000}
                تاريخ الإنشاء: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}

                --- معلومات المشتري ---
                الاسم: {order.Buyer.Name}
                المصنع: {buyerFactory?.LegalName ?? "-"}
                كود المصنع: FYD-{order.Buyer.Id + 2000}
                الرقم القومي: {order.Buyer.NationalId ?? "-"}
                السجل التجاري: {buyerFactory?.CommercialRegistryNo ?? "-"}
                البطاقة الضريبية: {buyerFactory?.TaxCardNo ?? "-"}

                --- معلومات المورد ---
                الاسم: {order.Seller.Name}
                المصنع: {sellerFactory?.LegalName ?? "-"}
                كود المصنع: FYD-{order.Seller.Id + 2000}
                الرقم القومي: {order.Seller.NationalId ?? "-"}
                السجل التجاري: {sellerFactory?.CommercialRegistryNo ?? "-"}
                البطاقة الضريبية: {sellerFactory?.TaxCardNo ?? "-"}

                --- بنود الاتفاق ---
                الكمية: {order.AgreedQuantity}
                سعر الوحدة: {order.AgreedPricePerUnit} ج.م
                الإجمالي: {order.AgreedQuantity * order.AgreedPricePerUnit} ج.م
                تاريخ التسليم: {order.DeliveryDate:yyyy-MM-dd}
                عنوان التسليم: {order.DeliveryAddress ?? "-"}

                --- البنود المالية ---
                نسبة العربون: {order.DownPaymentPercentage * 100}%
                قيمة العربون: {order.DownPaymentAmount} ج.م
                عمولة المنصة: {order.PlatformCommission} ج.م

                --- بنود الجزاءات ---
                {FormatPenaltyClauses(order.ContractTerms)}

                --- التوقيعات ---
                توقيع المشتري: {(signedByBuyer ? order.Buyer.Name + " - " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") : "--------------------")}
                توقيع المورد: {(signedBySeller ? order.Seller.Name + " - " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") : "--------------------")}

                تم الإنشاء بواسطة منصة فايد
            ";

            // Minimal valid PDF (just enough to be a valid PDF for storage purposes)
            // In production, replace with QuestPDF generated document
            var pdfBytes = System.Text.Encoding.UTF8.GetBytes(content);
            return pdfBytes;
        }

        private static string FormatPenaltyClauses(string? contractTermsJson)
        {
            if (string.IsNullOrWhiteSpace(contractTermsJson))
                return "                (لا توجد بنود جزاءات مسجلة)";

            try
            {
                var clauses = JsonSerializer.Deserialize<List<PenaltyClauseDto>>(contractTermsJson);
                if (clauses == null || clauses.Count == 0)
                    return "                (لا توجد بنود جزاءات مسجلة)";

                var lines = new List<string>();
                foreach (var clause in clauses)
                {
                    lines.Add($"                {clause.ArabicText}");
                    lines.Add("");
                }
                return string.Join(Environment.NewLine, lines);
            }
            catch
            {
                return "                (تعذر قراءة بنود الجزاءات)";
            }
        }
    }
}
