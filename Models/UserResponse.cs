namespace APIBackSpotify.Models
{
    /// <summary>
    /// Respuesta consulta de perfil
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Nombre de usuario definido en spotify
        /// </summary>
        public string? Username { get; set; }
        /// <summary>
        /// Url imagen de perfil
        /// </summary>
        public string? ImgUrl { get; set; }

        /// <summary>
        /// Puede ser rlacionado a ultimas escuhadas, artitas favoritos, canciones favoritas
        /// </summary>
        public List<ItemType>? Items { get; set; }

    }
}
