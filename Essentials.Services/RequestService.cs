using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Essentials.Services
{
    public class RequestService : IRequestService
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _baseUrl;

        public RequestService(string baseUrl)
        {
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore
            };

            _baseUrl = baseUrl;

            _serializerSettings.Converters.Add(new StringEnumConverter());
        }

        public async Task<TResult> DeleteAsync<TResult>(string uri, string token = "")
        {
            HttpClient httpClient = CreateHttpClient(token);
            HttpResponseMessage response = await httpClient.DeleteAsync(_baseUrl + uri);

            await HandleResponse(response);

            string serialized = await response.Content.ReadAsStringAsync();
            TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

            return result;
        }

        public async Task<TResult> GetAsync<TResult>(string uri, string token = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(token);
                HttpResponseMessage response = await httpClient.GetAsync(_baseUrl + uri);

                await HandleResponse(response);

                string serialized = await response.Content.ReadAsStringAsync();
                TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

                return result;
            }
            catch (Exception ex)
            {
                string message = $"URI: {uri}\nException: {ex}";
                Exception exp = new Exception(message);
                throw exp;
            }
        }

        public Task<TResult> PostAsync<TResult>(string uri, TResult data, string token = "")
        {
            return PostAsync<TResult, TResult>(uri, data, token);
        }

        public async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data, string token = "")
        {
            string serialized = string.Empty;
            try
            {
                HttpClient httpClient = CreateHttpClient(token);
                serialized = await Task.Run(() => JsonConvert.SerializeObject(data, _serializerSettings));
                HttpResponseMessage response = await httpClient.PostAsync(_baseUrl + uri, new StringContent(serialized, Encoding.UTF8, "application/json"));

                await HandleResponse(response);

                string responseData = await response.Content.ReadAsStringAsync();

                return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData, _serializerSettings));
            }
            catch (Exception ex)
            {
                string message = $"URI: {uri}\nData: {serialized}\nException: {ex}";
                Exception exp = new Exception(message);

                throw exp;
            }
        }

        public Task<TResult> PutAsync<TResult>(string uri, TResult data, string token = "")
        {
            return PutAsync<TResult, TResult>(uri, data, token);
        }

        public async Task<TResult> PutAsync<TRequest, TResult>(string uri, TRequest data, string token = "")
        {
            string serialized = string.Empty;
            try
            {
                HttpClient httpClient = CreateHttpClient(token);
                serialized = await Task.Run(() => JsonConvert.SerializeObject(data, _serializerSettings));
                HttpResponseMessage response = await httpClient.PutAsync(_baseUrl + uri, new StringContent(serialized, Encoding.UTF8, "application/json"));

                await HandleResponse(response);

                string responseData = await response.Content.ReadAsStringAsync();

                return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData, _serializerSettings));
            }
            catch (Exception ex)
            {
                string message = $"URI: {uri}\nData: {serialized}\nException: {ex}";
                Exception exp = new Exception(message);
                throw exp;
            }
        }

        private HttpClient CreateHttpClient(string token = "")
        {
            HttpClient httpClient = new HttpClient
            {
            };

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return httpClient;
        }


        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.Forbidden ||
                    response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception(content);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception(content);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception(content);
                }

                throw new HttpRequestException(content);
            }
        }
    }
}