using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Packpal.DAL.ModelViews;

public class DBConnectionString
{
	public string ConnectionStrings { get; set; } = string.Empty;
}
public class JWTToken
{
	public string? TokenString { get; set; }
	public string? Id { get; set; }
	public string? Email { get; set; }
	public string? Role { get; set; }
	public long ExpiresInMilliseconds { get; set; }
}

public class JwtModel
{
	public string? ValidAudience { get; set; }
	public string? ValidIssuer { get; set; }
	public string? ValidTester { get; set; }
	public string? SecretKey { get; set; }
}

public class TokenResponse
{
	[JsonPropertyName("access_token")]
	public string? AccessToken { get; set; }

	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; set; }

	[JsonPropertyName("token_type")]
	public string? TokenType { get; set; }

	[JsonPropertyName("id_token")]
	public string? IdToken { get; set; }

	[JsonPropertyName("refresh_token")]
	public string? RefreshToken { get; set; }
}

public class ApiResponse
{
	public string StatusCode { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public object? Data { get; set; }

}

public class UploadResponse
{
	public string FileUrl { get; set; } = string.Empty;
	public string FileName { get; set; } = string.Empty;
	public long FileSize { get; set; }
	public DateTime UploadDate { get; set; }
}

//Config
public class PayOSConfig
{
	public required string ApiKey { get; set; }
	public required string SecretKey { get; set; }
	public required string ClientId { get; set; }
	public required string ProdBaseUrl { get; set; }
	public string? WebhookUrl { get; set; }
}