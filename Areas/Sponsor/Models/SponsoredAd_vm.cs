namespace Trinbago_MVC5.Areas.Sponsor.Models
{
    public class SponsoredAdPhotoBase
    {
        public string StringId { get; set; }
    }

    public class SponsoredAdBase
    {
        public string StringId { get; set; }
        public string Name { get; set; }
        public SponsoredAdPhotoBase SponsoredPhoto { get; set; }
    }
}
