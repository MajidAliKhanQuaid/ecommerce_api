using Microsoft.AspNetCore.Authorization;

namespace freshstore.Authorization.Requirements.Category
{
    public class CanViewCategoriesRequirement : IAuthorizationRequirement
    {
        public CanViewCategoriesRequirement() { }
    }
}
