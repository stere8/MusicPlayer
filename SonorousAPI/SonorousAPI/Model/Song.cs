using System;
using System.ComponentModel.DataAnnotations;

namespace SonorousAPI.Models
{
    public class Song
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public string Genre { get; set; }

        public int Duration { get; set; }

        public string FileType { get; set; }
    }
}