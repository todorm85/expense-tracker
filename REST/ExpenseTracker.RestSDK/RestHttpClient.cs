using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ExpenseTracker.RestClient
{
    public class RestHttpClient : IHttpClient
    {
        private readonly HttpClient client;

        public RestHttpClient(string baseAddress, string authorizationHeader)
        {
            this.client = new HttpClient() { BaseAddress = new Uri(baseAddress) };
            if (authorizationHeader != null)
            {
                var authVals = authorizationHeader.Split(' ');
                this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authVals[0], authVals[1]);
            }
        }

        public RestHttpClient(string baseAddress) : this(baseAddress, null)
        {
        }

        public void Post(string endpointPath, string json)
        {
            var result = this.client.PostAsync(endpointPath, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            ValidateResult(result);
        }

        public string Get(string endpointPath)
        {
            var result = this.client.GetAsync(endpointPath).Result;
            ValidateResult(result);
            return result.Content.ReadAsStringAsync().Result;
        }

        public void Delete(string endpointPath)
        {
            var result = this.client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, endpointPath)).Result;
            ValidateResult(result);
        }

        public void Put(string endpointPath, string json)
        {
            var result = this.client.PutAsync(endpointPath, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            ValidateResult(result);
        }

        private void ValidateResult(HttpResponseMessage result)
        {
            if (!result.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"{result.StatusCode.ToString()} {result.Content.ReadAsStringAsync().Result}");
            }
        }
    }
}