using Microsoft.AspNetCore.Authorization;

namespace Fayed_API.Middlewares
{
    public class AdminOnlyRequirement : IAuthorizationRequirement { }

    public class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminOnlyRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
