using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LAB4_150348
{
    public class File
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }

        [Display(Name = "Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
}