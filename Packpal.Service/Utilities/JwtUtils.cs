using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Packpal.BLL.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews;

namespace Packpal.BLL.Utilities;

public class JwtUtils : IJwtUtils
{
	public JWTToken GenerateToken(IEnumerable<Claim> claims, JwtModel? jwtModel, User user)
	{
		var authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtModel?.SecretKey ?? ""));
		var expirationTime = DateTime.UtcNow.AddDays(1);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Issuer = jwtModel?.ValidIssuer,
			Audience = jwtModel?.ValidAudience,
			Expires = expirationTime,
			SigningCredentials = new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256),
			Subject = new ClaimsIdentity(claims)
		};
		var tokenHandler = new JwtSecurityTokenHandler();
		var token = tokenHandler.CreateToken(tokenDescriptor);
		var tokenString = tokenHandler.WriteToken(token);
		var jwtToken = new JWTToken
		{
			TokenString = tokenString,
			Id = user.Id.ToString(),				
			Email = user.Email,
			Role = user.ActiveRole ?? "RENTER",
			ExpiresInMilliseconds = (long)(expirationTime - DateTime.UtcNow).TotalMilliseconds
		};
		return jwtToken;
	}
}
