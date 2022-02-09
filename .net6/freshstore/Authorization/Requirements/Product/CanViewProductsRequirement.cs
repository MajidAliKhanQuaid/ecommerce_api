using Microsoft.AspNetCore.Authorization;

namespace freshstore.Authorization.Requirements.Product
{
    public class CanViewProductsRequirement : IAuthorizationRequirement
    {
        public CanViewProductsRequirement() { }

    }
}
