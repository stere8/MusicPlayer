using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SonorousAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SonorousAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSongs()
        {
            var songs = await _context.Songs.ToListAsync();

            if (!songs.Any())
            {
                return Ok(new { message = "No songs found." });
            }

            // Manually map Song to SongDTO
            var songDTOs = songs.Select(song => new SongDTO
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Genre = song.Genre,
                Duration = song.Duration,
                UploadDate = song.UploadDate
            }).ToList();

            return Ok(songDTOs);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string title, [FromForm] string artist, [FromForm] string album, [FromForm] string genre, [FromForm] int duration)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

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

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Convert to SongDTO to return after upload
            var songDTO = new SongDTO
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Genre = song.Genre,
                Duration = song.Duration,
                UploadDate = song.UploadDate
            };

            return Ok(songDTO);
        }
    }
}
