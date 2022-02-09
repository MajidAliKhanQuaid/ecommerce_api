using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Helpers
{
    //public class Jwt
    //{
    //    public Jwt(IOptions<Jwt>)
    //    {

    //    }

    //    public string CreatePlainJwt()
    //    {
    //        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    //        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    //        var claims = new List<Claim>();
    //        //
    //        var nameClaim = new Claim(ClaimTypes.Name, user.Name, ClaimValueTypes.String, issuer);
    //        var usernameClaim = new Claim(ClaimTypes.NameIdentifier, username, ClaimValueTypes.String, issuer);
    //        //
    //        claims.Add(nameClaim);
    //        claims.Add(usernameClaim);
    //        //
    //        DateTime expiry = DateTime.UtcNow.AddMinutes(5);
    //        //
    //        var token = new JwtSecurityToken(issuer,
    //          audience,
    //          claims,
    //          notBefore: DateTime.UtcNow,
    //          expires: DateTime.UtcNow.AddMinutes(5),
    //          signingCredentials: credentials);

    //        var handler = new JwtSecurityTokenHandler();

    //        /*
    //         * LEARN to Encrypt the Token
    //         */

    //        //var tokenDescriptor = new SecurityTokenDescriptor();
    //        //tokenDescriptor.Issuer = issuer;    
    //        //tokenDescriptor.Audience = audience;    
    //        //tokenDescriptor.Subject = new ClaimsIdentity(claims);
    //        //tokenDescriptor.EncryptingCredentials = new X509EncryptingCredentials(new X509Certificate2("key_public.cer"));


    //        //handler.

    //        string accessToken = handler.WriteToken(token);

    //        return accessToken;
    //    }
    //}
}
