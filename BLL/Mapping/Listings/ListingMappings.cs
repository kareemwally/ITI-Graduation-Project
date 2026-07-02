using BLL.DTOs.Listings;
using DAL.Models;

namespace BLL.Mapping.Listings
{
    public static class ListingMappings
    {
        public static ListingDto ToDto(this Listing e) => new()
        {
            Id = e.Id,
            FactoryAddress = e.Factory?.Address ?? string.Empty,
            CategoryName = e.Category?.Name,
            Title = e.Title,
            MaterialType = e.MaterialType,
            MaterialCondition = e.MaterialCondition,
            Quantity = e.Quantity,
            MeasureUnit = e.MeasureUnit,
            
            MinPrice = e.MinPrice,
            MaxPrice = e.MaxPrice,
            Status = e.Status,
            CreatedAt = e.CreatedAt,
            PublishedAt = e.PublishedAt,
            ExpiryDate = e.ExpiryDate
        };

        public static ListingDetailsDto ToDetailsDto(this Listing e) => new()
        {
            Id = e.Id,
            FactoryAddress = e.Factory?.Address ?? string.Empty,
            CategoryName = e.Category?.Name,
            Title = e.Title,
            MaterialType = e.MaterialType,
            MaterialCondition = e.MaterialCondition,
            Quantity = e.Quantity,
            MeasureUnit = e.MeasureUnit,
            MinPrice = e.MinPrice,
            MaxPrice = e.MaxPrice,
            Status = e.Status,
            CreatedAt = e.CreatedAt,
            PublishedAt = e.PublishedAt,
            ExpiryDate = e.ExpiryDate,
            Description = e.Description,
            MinOrderQuantity = e.MinOrderQuantity,
            IsNegotiable = e.IsNegotiable,
            IsDivisible = e.IsDivisible,
            DeliveryType = e.DeliveryType,
            PreferPayMethod = e.PreferPayMethod,
            CustomCatName = e.CustomCatName,
            
            VideoUrl = e.VideoUrl,
            CertificateUrl = e.CertificateUrl,
            Media = e.Media.Select(m => new ListingMediaDto
            {
                Id = m.Id,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                IsMain = m.IsMain
            }).ToList()
        };

        public static Listing ToEntity(this CreateListingDto dto) => new()
        {
            FactoryId = dto.FactoryId,
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Description = dto.Description,
            MaterialType = dto.MaterialType,
            MaterialCondition = dto.MaterialCondition,
            Quantity = dto.Quantity,
            MeasureUnit = dto.MeasureUnit,
            
            MinPrice = dto.MinPrice,
            MaxPrice = dto.MaxPrice,
            MinOrderQuantity = dto.MinOrderQuantity,
            IsNegotiable = dto.IsNegotiable,
            IsDivisible = dto.IsDivisible,
            DeliveryType = dto.DeliveryType,
            PreferPayMethod = dto.PreferPayMethod,
            CustomCatName = dto.CustomCatName,
            ExpiryDate = dto.ExpiryDate
        };

        public static void Apply(this UpdateListingDto dto, Listing e)
        {
            e.CategoryId = dto.CategoryId;
            e.Title = dto.Title;
            e.Description = dto.Description;
            e.MaterialType = dto.MaterialType;
            e.MaterialCondition = dto.MaterialCondition;
            e.Quantity = dto.Quantity;
            e.MeasureUnit = dto.MeasureUnit;
 
            e.MinPrice = dto.MinPrice;
            e.MaxPrice = dto.MaxPrice;
            e.MinOrderQuantity = dto.MinOrderQuantity;
            e.IsNegotiable = dto.IsNegotiable;
            e.IsDivisible = dto.IsDivisible;
            e.DeliveryType = dto.DeliveryType;
            e.PreferPayMethod = dto.PreferPayMethod;
            e.CustomCatName = dto.CustomCatName;
            e.Status = dto.Status;
            e.ExpiryDate = dto.ExpiryDate;
        }
    }
}