using Microsoft.AspNetCore.Http;
using Packpal.DAL.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.BLL.Interface;

public interface IFirebaseStorageService
{
	public Task<UploadResponse> UploadFileAsync(IFormFile file);
}
