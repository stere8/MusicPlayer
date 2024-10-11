using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SonorousAPI.Models; // Replace with your actual namespace where your models are defined

namespace SonorousAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolderPath;

        public SongsController(ApplicationDbContext context)
        {
            _context = context;
            _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
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
                UploadDate = song.UploadDate,
                FilePath = song.FilePath
            }).ToList();

            return Ok(songDTOs);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string title, [FromForm] string artist, [FromForm] string album, [FromForm] string genre, [FromForm] int duration)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_uploadsFolderPath, uniqueFileName);

            Directory.CreateDirectory(_uploadsFolderPath); // Ensure the uploads directory exists

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
                UploadDate = song.UploadDate,
                FilePath = song.FilePath
            };

            return Ok(songDTO);
        }

        [HttpGet("play/{filename}")]
        public IActionResult Play(string filename)
        {
            var filePath = Path.Combine(_uploadsFolderPath, filename);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var contentType = "audio/mpeg";
            var fileLength = file.Length;

            Response.Headers.Add("Accept-Ranges", "bytes");

            if (Request.Headers.ContainsKey("Range"))
            {
                var rangeHeader = Request.Headers["Range"].ToString();
                var range = rangeHeader.Replace("bytes=", "").Split('-');
                long from = long.Parse(range[0]);
                long to = range.Length > 1 && long.TryParse(range[1], out long end) ? end : fileLength - 1;

                if (from >= fileLength || to >= fileLength)
                {
                    return BadRequest("Requested range not satisfiable");
                }

                // Set file pointer to the starting point of the requested range
                file.Seek(from, SeekOrigin.Begin);
                var length = to - from + 1;

                Response.StatusCode = 206;
                Response.ContentLength = length;
                Response.Headers.Add("Content-Range", $"bytes {from}-{to}/{fileLength}");

                Console.WriteLine($"Serving range {from}-{to} of file {filename}, length {length}");
                return new FileStreamResult(file, contentType);
            }

            // If no range request is made, serve the entire file
            Response.ContentLength = fileLength;
            return new FileStreamResult(file, contentType);
        }


    }
}
