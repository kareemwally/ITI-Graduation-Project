using BLL.DTOs.Contracts;
using BLL.Mapping.Orders;
using BLL.Settings;
using DAL.Models;

namespace BLL.Mapping.Contracts
{
    public static class ContractMappings
    {
        public static string ToOrderCode(this int orderId) => $"FYD-{orderId + 2000}";

        public static PartyInfoDto ToPartyInfoDto(this User user, Factory? factory)
        {
            return new PartyInfoDto
            {
                UserId = user.Id,
                Name = user.Name,
                FactoryName = factory?.LegalName ?? string.Empty,
                FactoryCode = $"FYD-{user.Id + 2000}",
                NationalId = user.NationalId,
                CommercialRegistryNo = factory?.CommercialRegistryNo ?? string.Empty,
                TaxCardNo = factory?.TaxCardNo ?? string.Empty,
                LogoUrl = factory?.LogoUrl
            };
        }

        public static List<PenaltyClauseDto> BuildPenaltyClauses(ContractSettings settings)
        {
            return new List<PenaltyClauseDto>
            {
                new()
                {
                    Title = "force_majeure",
                    ArabicText = "لا يتحمل أي طرف المسؤولية أو الغرامة عن أي تأخير أو فشل في تنفيذ أي جزء من هذا العقد إذا كان هذا التأخير أو الفشل ناتجًا عن حدث قوة قاهرة، بما في ذلك على سبيل المثال لا الحصر الكوارث الطبيعية أو اللوائح الحكومية أو الإضرابات الصناعية الشديدة. وفي هذه الحالة، يجب على الطرف المتأثر إخطار الطرف الآخر وإدارة المنصة خلال 48 ساعة لتقييم تمديد أو إنهاء الطلب دون غرامات.",
                    Rate = null,
                    MaxCapRate = null,
                    TerminationDays = null
                },
                new()
                {
                    Title = "buyer_cancellation",
                    ArabicText = $"إذا طلب المشتري إلغاء الطلب بعد دفع العربون وقبل التسليم دون وجود خرق قانوني أو جودة من المورد، يخسر المشتري غرامة إلغاء بنسبة {settings.BuyerCancellationPenaltyRate * 100}% من قيمة العربون المدفوع. تذهب هذه القيمة للمورد كتعويض عن تخصيص الإنتاج وتكاليف تجهيز المواد.",
                    Rate = settings.BuyerCancellationPenaltyRate,
                    MaxCapRate = null,
                    TerminationDays = null
                },
                new()
                {
                    Title = "seller_quality",
                    ArabicText = $"يضمن المورد أن جميع البضائع الموردة تتوافق تمامًا مع المواصفات الفنية ومعايير الجودة المتفق عليها في هذا العقد. إذا تبين أن البضائع غير مطابقة أو معيبة أو ملوثة، يلتزم المورد بدفع غرامة عدم مطابقة الجودة بنسبة {settings.SellerQualityPenaltyRate * 100}% من قيمة الدفعة المخالفة. بالإضافة إلى ذلك، يتحمل المورد جميع تكاليف استبدال البضائع غير المطابقة أو تعديل الأسعار خلال 3 أيام عمل من تاريخ النزاع الرسمي.",
                    Rate = settings.SellerQualityPenaltyRate,
                    MaxCapRate = null,
                    TerminationDays = null
                },
                new()
                {
                    Title = "seller_delay",
                    ArabicText = $"إذا فشل المورد في تسليم الكميات المتفق عليها خلال تاريخ التسليم المحدد، تُحتسب غرامة تأخير بنسبة {settings.DelayPenaltyDailyRate * 100}% من إجمالي قيمة العقد عن كل يوم تأخير. لا يتجاوز إجمالي الغرامة المتراكمة حدًا أقصى قدره {settings.DelayPenaltyMaxCapRate * 100}% من إجمالي سعر العقد. إذا تجاوز التأخير {settings.DelayTerminationBusinessDays} أيام عمل، يحق للمشتري إنهاء العقد واسترداد كامل العربون المدفوع.",
                    Rate = settings.DelayPenaltyDailyRate,
                    MaxCapRate = settings.DelayPenaltyMaxCapRate,
                    TerminationDays = settings.DelayTerminationBusinessDays
                }
            };
        }

        public static ContractFormDto ToContractFormDto(this Order order, ContractSettings settings)
        {
            var totalPrice = order.AgreedQuantity * order.AgreedPricePerUnit;
            var downPayment = totalPrice * settings.DownPaymentPercentage;
            var commission = totalPrice * settings.CommissionRate;
            var sellerPayout = downPayment - commission;

            return new ContractFormDto
            {
                OrderId = order.Id,
                OrderCode = order.Id.ToOrderCode(),
                ListingId = order.ListingId,
                ListingTitle = order.Listing?.Title ?? string.Empty,
                Buyer = order.Buyer.ToPartyInfoDto(order.Buyer.Factory),
                Seller = order.Seller.ToPartyInfoDto(order.Seller.Factory),
                AgreedQuantity = order.AgreedQuantity,
                AgreedPricePerUnit = order.AgreedPricePerUnit > 0
                    ? order.AgreedPricePerUnit
                    : (order.AgreedTotalPrice / (order.AgreedQuantity > 0 ? order.AgreedQuantity : 1)),
                AgreedTotalPrice = totalPrice,
                DeliveryDate = order.DeliveryDate ?? DateTime.UtcNow.AddDays(30),
                DeliveryAddress = order.DeliveryAddress ?? order.Buyer.Factory?.Address ?? string.Empty,
                DownPaymentPercentage = settings.DownPaymentPercentage,
                DownPaymentAmount = downPayment,
                CommissionRate = settings.CommissionRate,
                PlatformCommission = commission,
                SellerTotalPayout = sellerPayout,
                PenaltyClauses = BuildPenaltyClauses(settings)
            };
        }

        public static ContractResponseDto ToContractResponseDto(this Order order, ContractSettings settings)
        {
            var totalPrice = order.AgreedQuantity * order.AgreedPricePerUnit;
            var downPayment = totalPrice * (order.DownPaymentPercentage > 0 ? order.DownPaymentPercentage : settings.DownPaymentPercentage);

            return new ContractResponseDto
            {
                OrderId = order.Id,
                OrderCode = order.Id.ToOrderCode(),
                ListingId = order.ListingId,
                ListingTitle = order.Listing?.Title ?? string.Empty,
                Status = order.Status.ToArabicStatus(),
                Buyer = order.Buyer.ToPartyInfoDto(order.Buyer.Factory),
                Seller = order.Seller.ToPartyInfoDto(order.Seller.Factory),
                AgreedQuantity = order.AgreedQuantity,
                AgreedPricePerUnit = order.AgreedPricePerUnit,
                AgreedTotalPrice = totalPrice,
                DeliveryDate = order.DeliveryDate,
                DeliveryAddress = order.DeliveryAddress,
                DownPaymentPercentage = order.DownPaymentPercentage > 0 ? order.DownPaymentPercentage : settings.DownPaymentPercentage,
                DownPaymentAmount = downPayment,
                IsDownPaymentPaid = order.IsDownPaymentPaid,
                CommissionRate = settings.CommissionRate,
                PlatformCommission = totalPrice * settings.CommissionRate,
                SellerTotalPayout = downPayment - (totalPrice * settings.CommissionRate),
                PenaltyClauses = BuildPenaltyClauses(settings),
                IsSignedByBuyer = order.IsSignedByBuyer,
                IsSignedBySeller = order.IsSignedBySeller,
                ContractGeneratedAt = order.ContractGeneratedAt,
                SellerSignedAt = order.SellerSignedAt,
                BuyerSignedAt = order.BuyerSignedAt,
                DeclineReason = order.DeclineReason,
                ContractUrl = order.ContractUrl
            };
        }

        public static PaymentSummaryDto ToPaymentSummaryDto(this Order order, ContractSettings settings)
        {
            var totalPrice = order.AgreedQuantity * order.AgreedPricePerUnit;
            var downPayment = totalPrice * (order.DownPaymentPercentage > 0 ? order.DownPaymentPercentage : settings.DownPaymentPercentage);

            return new PaymentSummaryDto
            {
                OrderId = order.Id,
                OrderCode = order.Id.ToOrderCode(),
                ListingTitle = order.Listing?.Title ?? string.Empty,
                AgreedQuantity = order.AgreedQuantity,
                AgreedPricePerUnit = order.AgreedPricePerUnit,
                AgreedTotalPrice = totalPrice,
                DownPaymentPercentage = order.DownPaymentPercentage > 0 ? order.DownPaymentPercentage : settings.DownPaymentPercentage,
                DownPaymentAmount = downPayment,
                CommissionRate = settings.CommissionRate
            };
        }
    }
}
