using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace SonorousAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : Controller
    {
        private string _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        [HttpPost("upload")] // Add the HttpPost attribute here
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                BadRequest("No File uploaded");
            }

            if (!Directory.Exists(_uploadsFolderPath))
            {
                Directory.CreateDirectory(_uploadsFolderPath);
            }

            var filePath = Path.Combine("uploads", file.FileName);

            await using(var stream = new FileStream(filePath,FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok( new {filePath});  
        }

        [HttpGet("play/{filename}")]
        public IActionResult Play(string filename)
        {
            var filePath = Path.Combine(_uploadsFolderPath,filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = "audio/mpeg";

            var memory = new MemoryStream();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            using (var fileStream1 = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream1.CopyTo(memory);
            }

            // Check for range request from the client
       
            

            // No range requested, return full content
            return new FileStreamResult(fileStream, contentType)
            {
                EnableRangeProcessing = true
            };
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Test passed!");
        }
    }
}
