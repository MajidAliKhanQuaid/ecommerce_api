using freshstore.Authorization.Requirements.Order;
using freshstore.bll.Consts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace freshstore.Authorization.Handlers.Order
{
    public class CanViewOrdersHandler : AuthorizationHandler<CanViewOrdersRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, CanViewOrdersRequirement requirement)
        {
            // either it should contains user's permission or admin's
            var canViewOrdersClaim = context.User.FindFirst(
                c => (c.Type == RolePermissionConsts.CAN_VIEW_USER_ORDERS || c.Type == UserPermissionConsts.CAN_VIEW_ORDERS) && c.Value == "1");

            if (canViewOrdersClaim is not null)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
