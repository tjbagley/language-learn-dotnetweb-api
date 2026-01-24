using System.ComponentModel.DataAnnotations;

namespace LanguageLearnNETWebAPI.Models
{
    public class WordBase
    {
        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; } = string.Empty;
        public string? SoundsLike { get; set; } = string.Empty;
        [Required]
        public string Meaning { get; set; } = string.Empty;
    }
}
