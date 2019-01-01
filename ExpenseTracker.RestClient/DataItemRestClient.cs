using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ExpenseTracker.RestClient
{
    public class DataItemRestClient<T> : IBaseDataItemService<T> where T : IDataItem
    {
        protected readonly HttpClient client;
        protected readonly string endpointPath;

        protected readonly DataContractJsonSerializerSettings serializerSettings = new DataContractJsonSerializerSettings()
        {
            DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("yyyy-MM-dd'T'HH:mm:sszzz"),
            UseSimpleDictionaryFormat = true
        };

        public DataItemRestClient(string baseAddress, string endpointPath)
        {
            this.client = new HttpClient() { BaseAddress = new Uri(baseAddress) }; ;
            this.endpointPath = endpointPath;
        }

        public void Add(IEnumerable<T> items)
        {
            var json = this.Serialize(items);
            var result = this.client.PostAsync(this.endpointPath, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            ValidateResult(result);
        }

        public IEnumerable<T> GetAll()
        {
            var response = this.client.GetStreamAsync(this.endpointPath);
            return this.Deserialize<List<T>>(response.Result);
        }

        public void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var result = this.client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, this.endpointPath + $"/{item.Id}")).Result;
                ValidateResult(result);
            }
        }

        public void Update(IEnumerable<T> items)
        {
            var json = this.Serialize(items);
            var result = this.client.PutAsync(this.endpointPath, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            ValidateResult(result);
        }

        protected static void ValidateResult(HttpResponseMessage result)
        {
            if (!result.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"{result.StatusCode.ToString()} {result.Content.ReadAsStringAsync().Result}");
            }
        }

        protected string Serialize<T1>(T1 items)
        {
            var serializer = new DataContractJsonSerializer(typeof(T1), this.serializerSettings);
            var memStream = new MemoryStream();
            serializer.WriteObject(memStream, items);
            memStream.Position = 0;
            var stReader = new StreamReader(memStream);
            var json = stReader.ReadToEnd();
            return json;
        }

        protected TDes Deserialize<TDes>(Stream str) where TDes : class
        {
            var serializer = new DataContractJsonSerializer(typeof(TDes), this.serializerSettings);
            return serializer.ReadObject(str) as TDes;
        }
    }
}