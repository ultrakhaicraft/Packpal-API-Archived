using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.BLL.Services;

namespace Packpal.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
	private readonly IFirebaseStorageService _storageService;

	public UploadController(IFirebaseStorageService storageService)
	{
		_storageService = storageService;
	}

	[Authorize]
	[HttpPost]
	public async Task<IActionResult> UploadFile(IFormFile file)
	{
		if (file == null || file.Length == 0)
			return BadRequest("No file selected.");

		//Impose a file size limit (e.g., 10 MB)
		if (file.Length > 10 * 1024 * 1024) 
			return BadRequest("File size exceeds the limit of 10 MB.");

		var url = await _storageService.UploadFileAsync(file);
		return Ok(new { fileUrl = url });
	}
}



