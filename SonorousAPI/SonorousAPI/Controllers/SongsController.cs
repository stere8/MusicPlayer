using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonorousAPI.Models;

namespace SonorousAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public SongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string title, [FromForm] string artist, [FromForm] string album, [FromForm] string genre, [FromForm] int duration)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!Directory.Exists(_uploadsFolderPath))
            {
                Directory.CreateDirectory(_uploadsFolderPath);
            }

            // Generate a unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Create the file path for saving the file
            var filePath = Path.Combine(_uploadsFolderPath, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save the song details to the database
            var song = new Song
            {
                Title = title,
                Artist = artist,
                Album = album,
                FilePath = uniqueFileName, // Store the unique file name only
                Genre = genre,
                Duration = duration,
                FileType = file.ContentType,
                UploadDate = DateTime.Now
            };

            try
            {
                _context.Songs.Add(song);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while saving to the database.");
            }

            Console.WriteLine($"File uploaded: {filePath}");

            // After upload, return the list of all songs
            return await GetAllSongs();
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSongs()
        {
            var songs = await _context.Songs.ToListAsync();

            if (!songs.Any())
            {
                return Ok(new { message = "No songs found." });
            }

            return Ok(songs);
        }
        [HttpGet("play/{filename}")]
        public IActionResult Play(string filename)
        {
            var filePath = Path.Combine(_uploadsFolderPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            // This will serve the file correctly with the appropriate headers.
            return PhysicalFile(filePath, "audio/mpeg", filename, false);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Test passed!!!");
        }
    }
}
