using ClassLibrary.Services;
using ClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace ABCRetail_POE.Controllers
{
    public class ContractController : Controller
    {

        private readonly FileService _fileService;

        //---------------------------------------------------------------------------------------------------------------------
        public ContractController(FileService fileService)
        {
            _fileService = fileService;
        }

        //---------------------------------------------------------------------------------------------------------------------
        // Displays a list of uploaded files
        public async Task<IActionResult>Index()
        {
            List<Contract> files;
            try
            {
                files = await _fileService.ListFileAsync("uploads");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Failed to load files : {ex.Message}";
                files = new List<Contract>();
            }
            return View(files);
        }

        //---------------------------------------------------------------------------------------------------------------------
        // Handles file upload logic
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to uplaod");
            }
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    string directoryName = "uploads";
                    string fileName = file.FileName;
                    await _fileService.UploadFileAsync(directoryName, fileName, stream);

                }
                TempData["Message"] = $"File '{file.FileName}' uplpaded successfully";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"File upload failed: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        //---------------------------------------------------------------------------------------------------------------------
        // Handles downloading a file
        public async Task<IActionResult>DownloadFile(string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be null or empty");

            }
            try
            {
                var fileStream = await _fileService.DownloadFileAsync("uploads", fileName);
                if (fileStream == null)
                {
                    return NotFound($"File '{fileName}' not found");
                }
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not download file:{ex.Message}");
            }
        }
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------