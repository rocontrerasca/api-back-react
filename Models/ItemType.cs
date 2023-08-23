namespace APIBackSpotify.Models
{
    /// <summary>
    /// Clase que representa tipos de elementos(Cancion, album, playlist, Artista)
    /// </summary>
    public class ItemType
    {
        /// <summary>
        /// Nombre elemento
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Cantante
        /// </summary>
        public string? Artist { get; set; }
        /// <summary>
        /// Url portada
        /// </summary>
        public string? ImgUrl { get; set; }
        /// <summary>
        /// Id interno spotify
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Typo de elemento(Artista, almbum, playlist, canción)
        /// </summary>
        public string? Type { get; set; }
    }
}
