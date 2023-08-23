namespace APIBackSpotify.Models
{
    public class SearchResponse
    {
        public List<ItemType>? Albums { get; set; }

        public List<ItemType>? Artists { get; set; }
        public List<ItemType>? Tracks { get; set; }
        public List<ItemType>? PlayList { get; set; }


    }
}
