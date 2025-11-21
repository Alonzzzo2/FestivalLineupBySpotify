using System.ComponentModel.DataAnnotations;

namespace FestivalLineupBySpotify_API.Configuration
{
    public class ClashFindersSettings
    {
        [Required(ErrorMessage = "ClashFinders AuthUsername is required.")]
        public string AuthUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "ClashFinders AuthPublicKey is required.")]
        public string AuthPublicKey { get; set; } = string.Empty;
    }
}