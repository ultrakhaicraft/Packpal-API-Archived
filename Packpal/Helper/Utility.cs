using System.Security.Cryptography;

namespace Packpal.Helper;

public class Utility
{
	private static readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
	private static readonly string[] permittedMimeTypes = { "image/jpeg", "image/png" };


	public static long GenerateSecureOrderCode()
	{
		using var rng = RandomNumberGenerator.Create();
		byte[] bytes = new byte[4];
		rng.GetBytes(bytes);
		int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF; // ensure non-negative
		return 100000 + (value % 900000); // ensures it's a 6-digit number
	}

	public static bool IsPdfFile(IFormFile? file)
	{
		if (file == null || file.Length == 0)
			return false;

		// Check file extension
		var fileName = file.FileName;
		if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
			return false;

		// Check MIME type
		if (file.ContentType != "application/pdf")
			return false;

		return true;
	}

	public static bool IsImageFile(IFormFile? file)
	{
		if (file == null || file.Length == 0)
			return false;

		var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
		var contentType = file.ContentType.ToLowerInvariant();

		return permittedExtensions.Contains(extension) && permittedMimeTypes.Contains(contentType);
	
	}
}
