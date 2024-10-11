using System;

namespace SonorousAPI.Models
{
    public class SongDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public int Duration { get; set; } // Duration in seconds
        public DateTime UploadDate { get; set; }
    }
}
