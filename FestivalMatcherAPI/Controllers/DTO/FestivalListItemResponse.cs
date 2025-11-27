namespace FestivalMatcherAPI.Controllers.DTO
{
    public class FestivalListItemResponse
    {
        public string Title { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int PrintAdvisory { get; set; }
    }
}
