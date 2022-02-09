using freshstore.Authorization.Requirements.Category;
using freshstore.bll.Consts;
using Microsoft.AspNetCore.Authorization;

namespace freshstore.Authorization.Handlers.Category
{
    public class CanViewCategoriesHandler : AuthorizationHandler<CanViewCategoriesRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, CanViewCategoriesRequirement requirement)
        {
            // either it should contains user's permission or admin's
            var canViewCategoriesClaim = context.User.FindFirst(
                c => (c.Type == RolePermissionConsts.CAN_VIEW_CATEGORIES || c.Type == UserPermissionConsts.CAN_VIEW_CATEGORIES) && c.Value == "1");

            if (canViewCategoriesClaim is not null)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
