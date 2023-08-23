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
    /// API que consulta las recomendaciones por usuario
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RecommendationsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

       /// <summary>
       /// Consultar recomendaciones para el usuario en sesión
       /// </summary>
       /// <param name="authorization">Token usuario</param>
       /// <param name="limit">Cantdad de registros a obtener</param>
       /// <param name="seedArtists">Id artistas como base de busqueda</param>
       /// <param name="market">Codigo pais para limitar busqueda</param>
       /// <returns>Listado de canciones recomendadas</returns>
        [HttpGet]
        public async Task<ActionResult> Get([FromHeader] string authorization, [FromQuery] string limit, [FromQuery] string seedArtists,
            [FromQuery] string market)
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

                //FormUrlEncodedContent requestBody = new FormUrlEncodedContent(formData);

                //Request Token
                var httpResponseMessage = await httpClient.GetAsync(string.Format("/v1/recommendations?limit={0}&seed_artists={1}&market={2}", limit, seedArtists, market));
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var userResponse = new UserResponse();
                userResponse.Items = new List<ItemType>();
                var jTokenResponse = JToken.Parse(response);
                var xJArray = (JArray)(jTokenResponse["tracks"] ?? new JArray());
                foreach (var item in xJArray)
                {
                    var jObject = (JObject)item;
                    var jObjectAlmbum = (JObject)(item["album"] ?? new JObject());
                    var arrayImages = (JArray)(jObjectAlmbum["images"] ?? new JArray());
                    var objectImage = (JObject)arrayImages[0];

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

    }
}