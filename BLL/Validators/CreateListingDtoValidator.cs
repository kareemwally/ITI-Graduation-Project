using BLL.DTOs.Listings;
using FluentValidation;

namespace BLL.Validators
{
    public class CreateListingDtoValidator : AbstractValidator<CreateListingDto>
    {
        public CreateListingDtoValidator()
        {
            RuleFor(x => x.FactoryId).GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty().MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty();

            RuleFor(x => x.MaterialType)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.MeasureUnit)
                .NotEmpty().MaximumLength(20);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.MinOrderQuantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(x => x.Quantity)
                .WithMessage("MinOrderQuantity cannot exceed the available quantity.");

            RuleFor(x => x.MaterialCondition).IsInEnum();
            RuleFor(x => x.DeliveryType).IsInEnum();
            RuleFor(x => x.PreferPayMethod).IsInEnum();

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("ExpiryDate must be in the future.");

            // A listing must resolve to either a known category or a custom category name.
            RuleFor(x => x)
                .Must(x => x.CategoryId.HasValue || !string.IsNullOrWhiteSpace(x.CustomCatName))
                .WithMessage("Provide a CategoryId or a CustomCatName.");
        }
    }
}
