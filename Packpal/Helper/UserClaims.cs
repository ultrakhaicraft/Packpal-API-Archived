using System.Security.Claims;

namespace Packpal.Helper;

//Get user info through token claims
public static class UserClaims
{
	public static string GetUserNameFromJwtToken(this IEnumerable<Claim> claims)
	{
		var userName = claims.FirstOrDefault(claims => claims.Type == ClaimTypes.Name)?.Value;
		return userName ?? "";
	}

	public static string GetUserIdFromJwtToken(this IEnumerable<Claim> claims)
	{
		var userId = claims.FirstOrDefault(claims => claims.Type == "id")?.Value;
		return userId ?? "";
	}

	public static string GetUserRoleFromJwtToken(this IEnumerable<Claim> claims)
	{
		var role = claims.FirstOrDefault(claims => claims.Type == ClaimTypes.Role)?.Value;
		return role ?? "";
	}
}
