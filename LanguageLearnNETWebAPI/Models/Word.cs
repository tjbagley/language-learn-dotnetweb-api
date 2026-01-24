using System.ComponentModel.DataAnnotations;

namespace LanguageLearnNETWebAPI.Models
{
    public class Word : WordBase
    {
        [Required(ErrorMessage = "Id is required")]
        public string? Id { get; set; }
    }
}
