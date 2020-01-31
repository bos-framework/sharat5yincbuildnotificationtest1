using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BOS.StarterCode.Helpers.HttpClientFactories
{
    public static class ApplicationSettings
    {
        public static string ApiBaseUrl { get; set; }
    }

    public partial class ApiClient
    {
        private readonly HttpClient _httpClient;
        private Uri BaseEndpoint { get; set; }
        
        public ApiClient(Uri baseEndpoint)
        {
            if (baseEndpoint == null)
            {
                throw new ArgumentNullException("baseEndpoint");
            }
            BaseEndpoint = baseEndpoint;
            _httpClient = new HttpClient();
        }

        private T GetSync<T>(Uri requestUrl)
        {
            //addHeaders();
            string data = "";
            using (var client = new HttpClient())
            {
                var response1 = client.GetAsync(requestUrl).Result;

                if (response1.IsSuccessStatusCode)
                {
                    var responseContent = response1.Content;
                    // by calling .Result you are synchronously reading the result
                     data = responseContent.ReadAsStringAsync().Result;
                   // Console.WriteLine(responseString);
                }
            }

            //var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
            //response.EnsureSuccessStatusCode();
            //var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }

        private async Task<T> GetAsync<T>(Uri requestUrl)
        {
            //addHeaders();
            var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Common method for making POST calls
        /// </summary>
        private async Task<ResponseBody<T>> PostAsync<T>(Uri requestUrl, T content)
        {
            //addHeaders();
            var response = await _httpClient.PostAsync(requestUrl.ToString(), CreateHttpContent<T>(content));
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResponseBody<T>>(data);
        }

        private async Task<ResponseBody<T1>> PostAsync<T1, T2>(Uri requestUrl, T2 content)
        {
            //addHeaders();
            var response = await _httpClient.PostAsync(requestUrl.ToString(), CreateHttpContent<T2>(content));
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResponseBody<T1>>(data);
        }

        private Uri CreateRequestUri(string relativePath, string queryString = "")
        {
            var endpoint = new Uri(BaseEndpoint, relativePath);
            var uriBuilder = new UriBuilder(endpoint);
            uriBuilder.Query = queryString;
            return uriBuilder.Uri;
        }

        private HttpContent CreateHttpContent<T>(T content)
        {
            var json = JsonConvert.SerializeObject(content, MicrosoftDateFormatSettings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static JsonSerializerSettings MicrosoftDateFormatSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };
            }
        }

        private void addHeaders()
        {
            _httpClient.DefaultRequestHeaders.Remove("Bearer");
            //_httpClient.DefaultRequestHeaders.Add("Bearer", _configuration["BOS:APIkey"]);
        }
    }

    [DataContract]
    public class ResponseBody<T>
    {
        [DataMember(Name = "IsSuccess")]
        public bool IsSuccess { get; set; }

        [DataMember(Name = "ReturnMessage")]
        public string ReturnMessage { get; set; }

        [DataMember(Name = "Data")]
        public T Data { get; set; }
    }


}
