using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SonorousAPI.Model;

namespace SonorousAPI.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class SongsController : Controller
    {
        private string _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        [Microsoft.AspNetCore.Mvc.HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No File uploaded");
            }

            if (!Directory.Exists(_uploadsFolderPath))
            {
                Directory.CreateDirectory(_uploadsFolderPath);
            }

            var filePath = Path.Combine(_uploadsFolderPath, file.FileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Console.WriteLine($"File uploaded: {filePath}");
            return Ok(new { filePath });
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("play/{filename}")]
        public IActionResult Play(string filename)
        {
            Console.WriteLine("2Play method called.");
            Console.WriteLine($"Filename: {filename}");

            var filePath = Path.Combine(_uploadsFolderPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            // This will serve the file correctly with the appropriate headers.
            return PhysicalFile(filePath, "audio/mpeg", filename, false);
        }

        
        public async Task WriteToStream(string filePath, Stream outputStream)
        {
            try
            {
                var buffer = new byte[65536];

                using (var video = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    var length = (int)video.Length;
                    int bytesRead;

                    while (length > 0)
                    {
                        bytesRead = await video.ReadAsync(buffer, 0, Math.Min(length, buffer.Length));
                        if (bytesRead == 0) break; // End of file
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                        length -= bytesRead;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                Console.WriteLine($"Error streaming file: {ex.Message}");
            }
        }


        [Microsoft.AspNetCore.Mvc.HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Test passed!");
        }
    }
}