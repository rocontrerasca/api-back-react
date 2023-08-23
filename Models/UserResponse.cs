namespace APIBackSpotify.Models
{
    public class UserResponse
    {
        public string? Username { get; set; }
        public string? ImgUrl { get; set; }

        public List<ItemType>? Items { get; set; }

    }
}
