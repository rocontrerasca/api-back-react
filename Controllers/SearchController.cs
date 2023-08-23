using APIBackSpotify.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APIBackSpotify.Controllers
{
    /// <summary>
    /// API que consulta coincidencias con elementos tipo canciones, albumes, artistas
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class SearchController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Buscar canciones, artistas, almbues, playlist por palabras especificas
        /// </summary>
        /// <param name="authorization">Token usuario</param>
        /// <param name="limit">Cantidad de registros a traer</param>
        /// <param name="q">Cadena de busqueda</param>
        /// <param name="type">Typo de elementos a buscar</param>
        /// <param name="market">Codigo de pais</param>
        /// <returns>Objeto con los resultados que coinciden para canciones, artitas, almbumes, playlist</returns>
        [HttpGet]
        public async Task<ActionResult> GetTopArtist([FromHeader] string authorization, [FromQuery] string limit, [FromQuery] string q,
            [FromQuery] string type, [FromQuery] string market)
        {
            dynamic responseE = new JObject();
            List<object> dataList = new();

            try
            {
                if (string.IsNullOrEmpty(authorization))
                    throw new UnauthorizedAccessException("No autorizado");

                var httpClient = _httpClientFactory.CreateClient("api_spotify");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization.Substring("Bearer ".Length));

                var httpResponseMessage = await httpClient.GetAsync(string.Format("/v1/search?limit={0}&q={1}&market={2}&type={3}", limit, q, market, type));
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var listItem = new SearchResponse
                {
                    PlayList = new List<ItemType>(),
                    Albums = new List<ItemType>(),
                    Tracks = new List<ItemType>(),
                    Artists = new List<ItemType>()
                };

                var jTokenResponse = JToken.Parse(response);
                var xJObjectAlbums = (JObject)(jTokenResponse["albums"] ?? new JObject());
                var xJObjectArtists = (JObject)(jTokenResponse["artists"] ?? new JObject());
                var xJObjectTracks = (JObject)(jTokenResponse["tracks"] ?? new JObject());
                var xJObjectPlaylist = (JObject)(jTokenResponse["playlists"] ?? new JObject());

                var xJArrayAlbums = (JArray)(xJObjectAlbums["items"] ?? new JArray());
                var xJArrayArtists = (JArray)(xJObjectArtists["items"] ?? new JArray());
                var xJArrayTracks = (JArray)(xJObjectTracks["items"] ?? new JArray());
                var xJArrayPlaylist = (JArray)(xJObjectPlaylist["items"] ?? new JArray());

                foreach (var item in xJArrayAlbums)
                {
                    var jObject = (JObject)item;
                    var arrayImages = (JArray)(jObject["images"] ?? new JArray());
                    var objectImage = (JObject)(arrayImages.Count > 0 ? arrayImages[0] : new JObject());
                    var arrayArtists = (JArray)(jObject["artists"] ?? new JArray());

                    ItemType itemType = new()
                    {
                        Artist = string.Join(" & ", arrayArtists.Select(a => $"{a["name"]}")),
                        ImgUrl = Convert.ToString(objectImage["url"] ?? string.Empty),
                        Name = Convert.ToString(jObject["name"] ?? string.Empty),
                        Id = Convert.ToString(jObject["id"] ?? string.Empty),
                        Type = "Album"
                    };

                    if (!listItem.Albums.Exists(ee => ee.Id.Equals(itemType.Id)))
                        listItem.Albums.Add(itemType);
                }

                foreach (var item in xJArrayArtists)
                {
                    var jObject = (JObject)item;
                    var arrayImages = (JArray)(jObject["images"] ?? new JArray());
                    var objectImage = (JObject)(arrayImages.Count > 0 ? arrayImages[0] : new JObject());

                    ItemType itemType = new()
                    {
                        Artist = string.Empty,
                        ImgUrl = Convert.ToString(objectImage["url"] ?? string.Empty),
                        Name = Convert.ToString(jObject["name"] ?? string.Empty),
                        Id = Convert.ToString(jObject["id"] ?? string.Empty),
                        Type = "Artista"
                    };

                    if (!listItem.Artists.Exists(ee => ee.Id.Equals(itemType.Id)))
                        listItem.Artists.Add(itemType);
                }

                foreach (var item in xJArrayTracks)
                {
                    var jObject = (JObject)item;
                    var jObjectAlmbum = (JObject)(item["album"] ?? new JObject());
                    var arrayImages = (JArray)(jObjectAlmbum["images"] ?? new JArray());
                    var objectImage = (JObject)(arrayImages.Count > 0 ? arrayImages[0] : new JObject());

                    var arrayArtists = (JArray)(jObjectAlmbum["artists"] ?? new JArray());

                    ItemType itemType = new()
                    {
                        Artist = string.Join(" & ", arrayArtists.Select(a => $"{a["name"]}")),
                        ImgUrl = Convert.ToString(objectImage["url"] ?? string.Empty),
                        Name = Convert.ToString(jObject["name"] ?? string.Empty),
                        Id = Convert.ToString(jObject["id"] ?? string.Empty),
                        Type = "Canción"
                    };

                    if (!listItem.Tracks.Exists(ee => ee.Id.Equals(itemType.Id)))
                        listItem.Tracks.Add(itemType);
                }

                foreach (var item in xJArrayPlaylist)
                {
                    var jObject = (JObject)item;
                    var arrayImages = (JArray)(jObject["images"] ?? new JArray());
                    var objectImage = (JObject)(arrayImages.Count > 0 ? arrayImages[0] : new JObject());

                    ItemType itemType = new()
                    {
                        Artist = string.Empty,
                        ImgUrl = Convert.ToString(objectImage["url"] ?? string.Empty),
                        Name = Convert.ToString(jObject["name"] ?? string.Empty),
                        Id = Convert.ToString(jObject["id"] ?? string.Empty),
                        Type = "Playlist"
                    };

                    if (!listItem.PlayList.Exists(ee => ee.Id.Equals(itemType.Id)))
                        listItem.PlayList.Add(itemType);
                }

                return StatusCode(statusCode: ((int)httpResponseMessage.StatusCode),
                    listItem);
            }
            catch (UnauthorizedAccessException e)
            {
                dataList.Add(new { code = e.HResult, message = e.Message, traceId = new Guid(Activity.Current.TraceId.ToString()) });
                responseE.errors = JToken.FromObject(dataList); ;
                return Unauthorized(responseE);
            }
            catch (Exception e)
            {
                dataList.Add(new { code = e.HResult, message = "Ha ocurrido un error internno ", traceId = new Guid(Activity.Current.TraceId.ToString()) });
                responseE.errors = JToken.FromObject(dataList);
                return StatusCode(StatusCodes.Status500InternalServerError, responseE);
            }
        }

    }
}