using freshstore.bll;
using freshstore.bll.Dtos;
using freshstore.bll.Models;
using freshstore.Config;
using freshstore.Model;
using freshstore.Requests;
using freshstore.Responses;
using freshstore.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("oauth/v{version:apiVersion}/auth")]
    public partial class AuthenticationController : ControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        private readonly FreshContext _context;
        private readonly TokenManagementService _tokenManagementService;

        public AuthenticationController(IOptions<JwtOptions> jwtOptions, FreshContext context, TokenManagementService tokenManagementService)
        {
            _jwtOptions = jwtOptions.Value;
            _context = context;
            _tokenManagementService = tokenManagementService;
        }

        [HttpPost("token/access")]
        [AllowAnonymous]
        public async Task<ActionResult> TokenRequest(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError(nameof(username), "Username must be valid");
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(nameof(password), "Password must be valid");
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Include(x => x.Permissions)
                .Include(x => x.Roles).ThenInclude(x => x.Permissions)
                .FirstOrDefaultAsync(x => x.Email == username && x.Password == password);
            if (user is null)
            {
                return Unauthorized("Provided email/password does not match users");
            }

            string issuer = _jwtOptions.Issuer;
            string audience = _jwtOptions.Audience;
            string secret = _jwtOptions.SigningSecret;
            /*
                Validate User Here i.e.
                bool isValid = _usersRepo.Where(x => x.UserName == userInfo.UserName && x.Password == userInfo.Password).Exists();
            */
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //
            var nameClaim = new Claim(ClaimTypes.Name, user.Name, ClaimValueTypes.String, issuer);
            var usernameClaim = new Claim(ClaimTypes.NameIdentifier, username, ClaimValueTypes.String, issuer);
            //var canAddProductClaim = new Claim("Manage.CanAddProduct", "1");
            //
            var refreshTokenClaims = new List<Claim>();
            refreshTokenClaims.Add(nameClaim);
            refreshTokenClaims.Add(usernameClaim);
            //refreshTokenClaims.Add(canAddProductClaim);

            var accessTokenClaims = GetAccessTokenClaims(user);
            //var accessTokenClaims = new List<Claim>();
            //accessTokenClaims.Add(nameClaim);
            //accessTokenClaims.Add(usernameClaim);
            //accessTokenClaims.Add(canAddProductClaim);

            var accessTokenInfo = _tokenManagementService.GetAccessToken(accessTokenClaims);
            //var accessTokenInfo = _tokenManagementService.GetEncryptedAccessToken(accessTokenClaims);
            var refreshTokenInfo = _tokenManagementService.GetRefreshToken(refreshTokenClaims);

            return Ok(new AccessTokenResponse
            {
                AccessToken = accessTokenInfo.Token,
                AccessTokenExpiresOnUtc = accessTokenInfo.ExpireOnUtc,
                RefreshToken = refreshTokenInfo.Token,
                RefreshTokenExpiresOnUtc = refreshTokenInfo.ExpireOnUtc
            });
        }

        [HttpPost("token/refresh")]
        public async Task<ActionResult> RefreshTokenRequest([FromBody] RefreshTokenRequest model)
        {
            var handler = new JwtSecurityTokenHandler();
            bool isReadable = handler.CanReadToken(model.RefreshToken);
            if (!isReadable)
            {
                return Unauthorized("Token is not readable");
            }

            var refreshTokenMetadata = handler.ReadJwtToken(model.RefreshToken);
            var username = refreshTokenMetadata.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Username is empty");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == username);
            if (user is null)
            {
                return Unauthorized("Provided email/password does not match users");
            }

            return Ok(new ResponseMessage { Success = true, Data = new { } });
        }

        //private List<string> GetRolePermissions()
        //{
        //    return typeof(RolePermissionsConts)
        //            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        //            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
        //            .Select(x => (string)x.GetRawConstantValue())
        //            .ToList();
        //}

        private List<Claim> GetAccessTokenClaims(User user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Email));
            claims.Add(new Claim(ClaimTypes.UserData, user.Id.ToString()));

            // role permissions
            if (user.Roles != null)
            {
                Dictionary<string, RoleLevelPermission> permission = new Dictionary<string, RoleLevelPermission>();
                var rolePermissions = user.Roles.Select(x => x.Permissions).SelectMany(x => x).DistinctBy(x => x.Name).ToList();
                foreach (var rolePermission in rolePermissions)
                {
                    // here validate permissions
                    claims.Add(new Claim(rolePermission.Name, "1"));
                }
            }

            // user permissions
            if (user.Permissions != null)
            {
                Dictionary<string, RoleLevelPermission> permission = new Dictionary<string, RoleLevelPermission>();
                var userPermissions = user.Permissions.DistinctBy(x => x.Name).ToList();
                foreach (var userPermission in userPermissions)
                {
                    // here validate permissions
                    claims.Add(new Claim(userPermission.Name, "1"));
                }
            }

            return claims;
        }

        private List<Claim> GetRefreshTokenClaims(RefreshTokenClaimInfo info)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, info.DisplayName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, info.Username));
            claims.Add(new Claim(ClaimTypes.UserData, info.AccountKey));

            return claims;
        }

    }
}
