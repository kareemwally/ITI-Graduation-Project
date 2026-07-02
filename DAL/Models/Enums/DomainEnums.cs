namespace DAL.Models.Enums
{
    /// <summary>Verification state shared by Users and Factories (KYB).</summary>
    public enum VerificationStatus
    {
        Pending,
        Verified,
        Rejected
    }

    /// <summary>Lifecycle of a single factory verification case.</summary>
    public enum VerificationCaseStatus
    {
        Pending,
        UnderReview,
        Decided
    }

    /// <summary>Final human decision on a verification case.</summary>
    public enum VerificationDecision
    {
        Approved,
        Rejected
    }

    /// <summary>Recommendation produced by the AI verification model.</summary>
    public enum AIRecommendation
    {
        Approve,
        Review,
        Reject
    }

    /// <summary>Physical condition of the listed material.</summary>
    public enum MaterialCondition
    {
        New,
        Used,
        Scrap,
        ByProduct
    }

    /// <summary>Who handles delivery for a listing / order.</summary>
    public enum DeliveryType
    {
        SellerDelivery,
        BuyerPickup,
        Both
    }

    /// <summary>Preferred payment method on a listing.</summary>
    public enum PaymentMethod
    {
        BankTransfer,
        Cash,
        Both
    }

    /// <summary>Publication state of a listing.</summary>
    public enum ListingStatus
    {
        Draft,
        Published,
        Sold,
        Expired
    }

    /// <summary>Type of a listing media asset.</summary>
    public enum MediaType
    {
        Image,
        Video
    }

    /// <summary>State of a buyer/seller chat thread.</summary>
    public enum ChatStatus
    {
        Open,
        Closed,
        Archived
    }

    /// <summary>Kind of message inside a chat.</summary>
    public enum MessageType
    {
        Text,
        Offer,
        System
    }

    /// <summary>Lifecycle of a confirmed order.</summary>
    public enum OrderStatus
    {
        PendingPayment,
        BuyerPaid,
        PayoutSent,
        Completed,
        Cancelled
    }

    /// <summary>Which party proposed a modification during negotiation.</summary>
    public enum PartyRole
    {
        Buyer,
        Seller
    }

    /// <summary>Type of a wallet ledger movement.</summary>
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Freeze,
        Unfreeze,
        Commission,
        Refund
    }

    /// <summary>State of a wallet transaction.</summary>
    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Reversed
    }
    public enum DisputeStatus
    {
        Open,
        underReview,
        Resolved,
        closed
    }

    public enum DisputeReason
    {
        QualityIssue,
        delay,
        Nonpayment,
        wrongQuantity,
        Other
    }

    public enum OfferStatus
    {
        pending,
        Accepted,
        Regjected,
        Withdrawn
    }



}
