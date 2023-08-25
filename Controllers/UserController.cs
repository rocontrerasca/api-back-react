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
    /// API que consulta perfil de usuario y elementos asociados a un usuario
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class UserController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Obtener el listado de artistas principales por usuario
        /// </summary>
        /// <param name="authorization">token generado</param>
        /// <param name="limit">Cantidad de datos</param>
        /// <returns>Httpcode con datos de artista</returns>
        [HttpGet("top/artists")]
        public async Task<ActionResult> GetTopArtist([FromHeader] string authorization, [FromQuery] int limit)
        {
            dynamic responseE = new JObject();
            List<object> dataList = new();

            try
            {
                if (string.IsNullOrEmpty(authorization))
                    throw new UnauthorizedAccessException("No autorizado");

                var httpClient = _httpClientFactory.CreateClient("api_spotify");
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("URL_SPOTIFY"));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization.Substring("Bearer ".Length));

                //FormUrlEncodedContent requestBody = new FormUrlEncodedContent(formData);

                //Request Token
                var httpResponseMessage = await httpClient.GetAsync("/v1/me/top/artists?limit=" + limit);
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var userResponse = new UserResponse();
                userResponse.Items = new List<ItemType>();
                var jTokenResponse = JToken.Parse(response);
                var xJArray = (JArray)(jTokenResponse["items"] ?? new JArray());
                foreach (var item in xJArray)
                {
                    var jObjectArtist = (JObject)item;
                    var arrayImages = (JArray)(jObjectArtist["images"] ?? new JArray());
                    var objectImage = (JObject)(arrayImages.Count > 0 ? arrayImages[0] : new JObject());

                    ItemType itemType = new()
                    {
                        Artist = Convert.ToString(jObjectArtist["name"] ?? string.Empty),
                        ImgUrl = Convert.ToString(objectImage["url"] ?? string.Empty),
                        Name = Convert.ToString(jObjectArtist["name"] ?? string.Empty),
                        Id = Convert.ToString(jObjectArtist["id"] ?? string.Empty),
                        Type = "Artista"
                    };

                    if (!userResponse.Items.Exists(ee => ee.Id.Equals(itemType.Id)))
                        userResponse.Items.Add(itemType);
                }

                return StatusCode(statusCode: ((int)httpResponseMessage.StatusCode),
                    userResponse);
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

        /// <summary>
        /// Obtener el listado de canciones recientemente escuchadas
        /// </summary>
        /// <param name="authorization">token generado</param>
        /// <param name="limit">Cantidad de datos</param>
        /// <returns>Httpcode con datos de canciones</returns>
        [HttpGet("player/recently-played")]
        public async Task<ActionResult> GetRecentlyPlayer([FromHeader] string authorization, [FromQuery] int limit)
        {
            dynamic responseE = new JObject();
            List<object> dataList = new();

            try
            {
                if (string.IsNullOrEmpty(authorization))
                    throw new UnauthorizedAccessException("No autorizado");

                var httpClient = _httpClientFactory.CreateClient("api_spotify");
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("URL_SPOTIFY"));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization.Substring("Bearer ".Length));

                var httpResponseMessage = await httpClient.GetAsync("/v1/me/player/recently-played?limit=" + limit);
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var userResponse = new UserResponse();
                userResponse.Items = new List<ItemType>();
                var jTokenResponse = JToken.Parse(response);
                var xJArray = (JArray)(jTokenResponse["items"] ?? new JArray());
                foreach (var item in xJArray)
                {
                    var jObjectTrack = (JObject)(item["track"] ?? new JObject());
                    var jObjectAlmbum = (JObject)(jObjectTrack["album"] ?? new JObject());
                    var arrayImages = (JArray)(jObjectAlmbum["images"] ?? new JArray());
                    var objectImage = (JObject)(arrayImages.Count > 0 ? arrayImages[0] : new JObject());

                    var arrayArtists = (JArray)(jObjectTrack["artists"] ?? new JArray());

                    ItemType itemType = new()
                    {
                        Artist = string.Join(" & ", arrayArtists.Select(a => $"{a["name"]}")),
                        ImgUrl = Convert.ToString(objectImage["url"] ?? string.Empty),
                        Name = Convert.ToString(jObjectTrack["name"] ?? string.Empty),
                        Id = Convert.ToString(jObjectTrack["id"] ?? string.Empty),
                        Type = "Canción"
                    };

                    if (!userResponse.Items.Exists(ee => ee.Id.Equals(itemType.Id)))
                        userResponse.Items.Add(itemType);
                }

                return StatusCode(statusCode: ((int)httpResponseMessage.StatusCode),
                    userResponse);
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

        /// <summary>
        /// Obtener el listado de canciones principales por usuario
        /// </summary>
        /// <param name="authorization">token generado</param>
        /// <param name="limit">Cantidad de datos</param>
        /// <returns>Httpcode con datos de canciones</returns>
        [HttpGet("top/tracks")]
        public async Task<ActionResult> GetTopTracks([FromHeader] string authorization, [FromQuery] int limit)
        {
            dynamic responseE = new JObject();
            List<object> dataList = new();

            try
            {
                if (string.IsNullOrEmpty(authorization))
                    throw new UnauthorizedAccessException("No autorizado");

                var httpClient = _httpClientFactory.CreateClient("api_spotify");
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("URL_SPOTIFY"));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization.Substring("Bearer ".Length));

                //FormUrlEncodedContent requestBody = new FormUrlEncodedContent(formData);

                //Request Token
                var httpResponseMessage = await httpClient.GetAsync("/v1/me/top/tracks?limit=" + limit);
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var userResponse = new UserResponse();
                userResponse.Items = new List<ItemType>();
                var jTokenResponse = JToken.Parse(response);
                var xJArray = (JArray)(jTokenResponse["items"] ?? new JArray());
                foreach (var item in xJArray)
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

                    if (!userResponse.Items.Exists(ee => ee.Id.Equals(itemType.Id)))
                        userResponse.Items.Add(itemType);
                }

                return StatusCode(statusCode: ((int)httpResponseMessage.StatusCode),
                    userResponse);
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

        /// <summary>
        /// Obtener información del usuario
        /// </summary>
        /// <param name="authorization">token generado</param>
        /// <returns>Httpcode con datos de usuario</returns>
        [HttpGet("profile")]
        public async Task<ActionResult> GetUser([FromHeader] string authorization)
        {
            dynamic responseE = new JObject();
            List<object> dataList = new();

            try
            {
                if (string.IsNullOrEmpty(authorization))
                    throw new UnauthorizedAccessException("No autorizado");

                var httpClient = _httpClientFactory.CreateClient("api_spotify");
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("URL_SPOTIFY"));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization.Substring("Bearer ".Length));

                //FormUrlEncodedContent requestBody = new FormUrlEncodedContent(formData);

                //Request Token
                var httpResponseMessage = await httpClient.GetAsync("/v1/me");
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var jTokenResponse = JToken.Parse(response);
                var xJArray = (JArray)(jTokenResponse["images"] ?? new JArray());
                var firstJTokenInxJArray = (JObject)(xJArray.Count > 0 ? xJArray[0] : new JObject());
                return StatusCode(statusCode: ((int)httpResponseMessage.StatusCode),
                    new UserResponse()
                    {
                        Username = jTokenResponse.Value<string?>("display_name") ?? string.Empty,
                        ImgUrl = Convert.ToString(firstJTokenInxJArray["url"] ?? string.Empty),
                    });
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