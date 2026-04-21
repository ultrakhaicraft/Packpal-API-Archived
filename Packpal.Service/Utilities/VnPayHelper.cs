using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Packpal.BLL.Utilities;

public class VnPayHelper
{
	
	private SortedList<String, String> _requestData = new SortedList<String, String>(new VnPayCompare());
	private SortedList<String, String> _responseData = new SortedList<String, String>(new VnPayCompare());

	public void AddRequestData(string key, string value)
	{
		if (!String.IsNullOrEmpty(value))
		{
			_requestData.Add(key, value);
		}
	}

	public void AddResponseData(string key, string value)
	{
		if (!String.IsNullOrEmpty(value))
		{
			_responseData.Add(key, value);
		}
	}

	public string GetResponseData(string key)
	{
		string retValue;
		if (_responseData.TryGetValue(key, out retValue))
		{
			return retValue;
		}
		else
		{
			return string.Empty;
		}
	}

	#region Request

	public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
	{
		StringBuilder data = new StringBuilder();
		foreach (KeyValuePair<string, string> kv in _requestData)
		{
			if (!String.IsNullOrEmpty(kv.Value))
			{
				data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
			}
		}
		string queryString = data.ToString();

		baseUrl += "?" + queryString;
		String signData = queryString;
		if (signData.Length > 0)
		{

			signData = signData.Remove(data.Length - 1, 1);
		}
		string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
		baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

		return baseUrl;
	}



	#endregion

	#region Response process

	public bool ValidateSignature(string inputHash, string secretKey)
	{
		string rspRaw = GetResponseData();
		string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
		return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
	}
	private string GetResponseData()
	{

		StringBuilder data = new StringBuilder();
		if (_responseData.ContainsKey("vnp_SecureHashType"))
		{
			_responseData.Remove("vnp_SecureHashType");
		}
		if (_responseData.ContainsKey("vnp_SecureHash"))
		{
			_responseData.Remove("vnp_SecureHash");
		}
		foreach (KeyValuePair<string, string> kv in _responseData)
		{
			if (!String.IsNullOrEmpty(kv.Value))
			{
				data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
			}
		}
		//remove last '&'
		if (data.Length > 0)
		{
			data.Remove(data.Length - 1, 1);
		}
		return data.ToString();
	}

	#endregion
}

public static class Utils
{
	public static string HmacSHA512(string key, string inputData)
	{
		var hash = new StringBuilder();
		byte[] keyBytes = Encoding.UTF8.GetBytes(key);
		byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
		using (var hmac = new HMACSHA512(keyBytes))
		{
			byte[] hashValue = hmac.ComputeHash(inputBytes);
			foreach (var theByte in hashValue)
			{
				hash.Append(theByte.ToString("x2"));
			}
		}

		return hash.ToString();
	}

	/// <summary>
	/// Lấy địa chỉ IP từ HttpContext của API Controller.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static string GetClientIpAddress(this HttpContext context)
	{
		var remoteIpAddress = context.Connection.RemoteIpAddress;

		if (remoteIpAddress != null)
		{
			var ipv4Address = Dns.GetHostEntry(remoteIpAddress).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

			return remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6 && ipv4Address != null
				? ipv4Address.ToString()
				: remoteIpAddress.ToString();
		}

		throw new InvalidOperationException("Không tìm thấy địa chỉ IP");
	}
}

public class VnPayCompare : IComparer<string>
{
	public int Compare(string x, string y)
	{
		if (x == y) return 0;
		if (x == null) return -1;
		if (y == null) return 1;
		var vnpCompare = CompareInfo.GetCompareInfo("en-US");
		return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
	}
}



/*
	/// <summary>
	/// Lấy địa chỉ IP từ HttpContext của API Controller.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static string GetClientIpAddress(this HttpContext context)
	{
		// Check if the IP is forwarded from a proxy
		var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

		if (string.IsNullOrWhiteSpace(ip))
		{
			// Fall back to direct connection IP
			ip = context.Connection.RemoteIpAddress?.ToString();
		}
		else
		{
			// X-Forwarded-For can contain multiple IPs: client, proxy1, proxy2,...
			ip = ip.Split(',').First().Trim();
		}

		return ip ?? "127.0.0.1"; // fallback
	}
*/
