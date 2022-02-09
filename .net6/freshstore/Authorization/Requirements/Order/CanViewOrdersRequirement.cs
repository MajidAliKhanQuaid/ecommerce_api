using Microsoft.AspNetCore.Authorization;

namespace freshstore.Authorization.Requirements.Order
{
    public class CanViewOrdersRequirement : IAuthorizationRequirement
    {
        public CanViewOrdersRequirement() { }

    }
}
