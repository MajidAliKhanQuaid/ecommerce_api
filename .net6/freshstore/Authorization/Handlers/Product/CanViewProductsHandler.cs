using freshstore.Authorization.Requirements.Product;
using freshstore.bll.Consts;
using Microsoft.AspNetCore.Authorization;

namespace freshstore.Authorization.Handlers.Product
{
    public class CanViewProductsHandler : AuthorizationHandler<CanViewProductsRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, CanViewProductsRequirement requirement)
        {
            // either it should contains user's permission or admin's
            var canViewOrdersClaim = context.User.FindFirst(
                c => (c.Type == RolePermissionConsts.CAN_VIEW_PRODUCTS || c.Type == UserPermissionConsts.CAN_VIEW_PRODUCTS) && c.Value == "1");

            if (canViewOrdersClaim is not null)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
