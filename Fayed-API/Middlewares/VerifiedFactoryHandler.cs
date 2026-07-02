using DAL.Models;
using DAL.Models.ExceptionModels;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Fayed_API.Middlewares
{

    //[Authorize(Policy = "VerifiedFactoryOnly")]
    public class VerifiedFactoryRequirement : IAuthorizationRequirement { }

    public class VerifiedFactoryHandler : AuthorizationHandler<VerifiedFactoryRequirement>
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerifiedFactoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            VerifiedFactoryRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                // لو التوكن مش سليم أو مفيش يوزر، سيب الـ .NET يرجع 401 Unauthorized الطبيعي
                return;
            }

            // جلب حالة التوثيق من الداتا بيز
            var factory = await _unitOfWork.Repository<Factory>()
                .FirstOrDefaultAsync(f => f.UserId == userId);

            // لو موثق، نجاح ويعدي بسلام
            if (factory != null && factory.VerificationStatus == DAL.Models.Enums.VerificationStatus.Verified)
            {
                context.Succeed(requirement);
            }
            else
            {
                throw new UnverifiedFactoryException();
            }
        }
    }
}
