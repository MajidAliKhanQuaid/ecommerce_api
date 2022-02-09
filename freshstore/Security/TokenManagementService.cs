using freshstore.Config;
using freshstore.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace freshstore.Security
{
    public class TokenManagementService
    {
        private readonly JwtOptions _jwtOptions;

        public TokenManagementService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public TokenInfo GetAccessToken(List<Claim> claims)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = CreateAccessToken(claims);
            string token = handler.WriteToken(securityToken);
            return new TokenInfo { Token = token, ExpireOnUtc = securityToken.ValidTo };
        }
        
        public TokenInfo GetEncryptedAccessToken(List<Claim> claims)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor securityToken = CreateEncryptedAccessToken(claims);
            string token = handler.CreateEncodedJwt(securityToken);
            return new TokenInfo { Token = token, ExpireOnUtc = DateTime.UtcNow }; // needs change here
        }

        public TokenInfo GetRefreshToken(List<Claim> claims)
        {

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = CreateRefreshToken(claims);
            string token = handler.WriteToken(securityToken);
            return new TokenInfo { Token = token, ExpireOnUtc = securityToken.ValidTo };
        }

        private JwtSecurityToken CreateAccessToken(List<Claim> claims)
        {
            string issuer = _jwtOptions.Issuer;
            string audience = _jwtOptions.Audience;
            string secret = _jwtOptions.SigningSecret;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(issuer,
              audience,
              claims,
              notBefore: DateTime.UtcNow,
              expires: DateTime.UtcNow.AddDays(30),
              signingCredentials: credentials);

            return securityToken;
        }
        
        private SecurityTokenDescriptor CreateEncryptedAccessToken(List<Claim> claims)
        {
            string issuer = _jwtOptions.Issuer;
            string audience = _jwtOptions.Audience;
            string signSecret = _jwtOptions.SigningSecret;
            string encSecret = _jwtOptions.SigningSecret;// _jwtOptions.EncryptionSecret;

            var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signSecret));
            var encKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encSecret));

            var signCredentials = new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256);
            var encCredentials = new EncryptingCredentials(encKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);

            var securityToken = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = signCredentials,
                EncryptingCredentials = encCredentials
                //EncryptingCredentials = new X509EncryptingCredentials(new X509Certificate2("key_public.cer"))
            };

            return securityToken;
        }

        private JwtSecurityToken CreateRefreshToken(List<Claim> claims)
        {
            string issuer = _jwtOptions.Issuer;
            string audience = _jwtOptions.Audience;
            string secret = _jwtOptions.SigningSecret;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(issuer,
              audience,
              claims,
              notBefore: DateTime.UtcNow,
              expires: DateTime.UtcNow.AddYears(1),
              signingCredentials: credentials);

            return securityToken;
        }

    }
}
