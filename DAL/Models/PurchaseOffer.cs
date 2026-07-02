using DAL.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Enums;
namespace DAL.Models
{
    public class PurchaseOffer : BaseEntity,ISoftDeletable
    {
        
        public int ListingId { get; set; }
        public Listing Listing { get; set; } = null!;

        // ربط العرض بالمشتري اللي بعته (نفترض إن الـ ID نوعه string)
        public  int  BuyerId { get; set; } 
        public User Buyer { get; set; } = null!;

        // تفاصيل العرض بناءً على السكرين شوت
        public decimal RequestedQuantity { get; set; }
        public decimal OfferedPricePerTon { get; set; }

        // إجمالي القيمة (ممكن نخزنها عشان نوفر حسابات بعدين)
        public decimal TotalValue { get; set; }

        // رسالة للمورد (اختياري)
        public string? BuyerMessage { get; set; }

        // حالة العرض
        public OfferStatus Status { get; set; } = OfferStatus.pending;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    // الـ Enum بتاع حالات العرض (ممكن تحطه في ملف Enums منفصل لو متعودين على كده)
  

}

