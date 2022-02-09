using Microsoft.AspNetCore.Authorization;

namespace freshstore.Authorization.Requirements
{
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public MinimumAgeRequirement(int minimumAge) =>
            MinimumAge = minimumAge;

        public int MinimumAge { get; }
    }
}
