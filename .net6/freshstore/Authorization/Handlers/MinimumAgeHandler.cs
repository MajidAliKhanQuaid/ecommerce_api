using freshstore.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace freshstore.Authorization.Handlers
{
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var dateOfBirthClaim = context.User.FindFirst(
                c => c.Type == ClaimTypes.DateOfBirth && c.Issuer == "http://contoso.com");

            if (dateOfBirthClaim is null)
            {
                return Task.CompletedTask;
            }

            //var dateOfBirth = Convert.ToDateTime(dateOfBirthClaim.Value);
            if (DateTime.TryParse(dateOfBirthClaim.Value, out var dateOfBirth))
            {
                int calculatedAge = DateTime.Today.Year - dateOfBirth.Year;
                if (dateOfBirth > DateTime.Today.AddYears(-calculatedAge))
                {
                    calculatedAge--;
                }

                if (calculatedAge >= requirement.MinimumAge)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
