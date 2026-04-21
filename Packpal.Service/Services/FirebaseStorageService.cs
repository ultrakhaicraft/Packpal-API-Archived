using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Packpal.BLL.Interface;
using Packpal.DAL.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.BLL.Services;

public class FirebaseStorageService : IFirebaseStorageService
{
	private readonly StorageClient _storageClient;
	private readonly string _bucketName;

	public FirebaseStorageService(string credentialsPath, string bucketName)
	{
		/*Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);*/
		GoogleCredential credential = GoogleCredential.FromFile(credentialsPath);
		_storageClient = StorageClient.Create(credential);
		_bucketName = bucketName;
	}

	public async Task<UploadResponse> UploadFileAsync(IFormFile file)
	{
		try { 


			
		
			//Only allow 10 MB pdf file size
			if(file.Length > 10*1024*1024)
			{
				throw new ApplicationException("File size must not be more than 10 MB");
			}

			var fileName = $"{Guid.NewGuid()}_{file.FileName}";
			using var stream = file.OpenReadStream();

			await _storageClient.UploadObjectAsync(_bucketName, fileName, null, stream, new UploadObjectOptions
			{
				PredefinedAcl = PredefinedObjectAcl.PublicRead // make it public
			});

			var fileUrl= $"https://storage.googleapis.com/{_bucketName}/{fileName}";

			var response = new UploadResponse
			{
				FileUrl = fileUrl,
				FileName = fileName,
				FileSize = file.Length,
				UploadDate = DateTime.UtcNow
			};

			return response;
		}
		catch(Exception e)
		{
			// Log the exception (not implemented here)
			Console.WriteLine("Exception: " + e.Message);
			Console.WriteLine("StackTrace: " + e.StackTrace);
			throw;
		}
	}
}
