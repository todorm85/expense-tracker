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
        protected readonly IHttpClient client;
        protected string EndpointPath { get; set; }

        protected readonly DataContractJsonSerializerSettings serializerSettings = new DataContractJsonSerializerSettings()
        {
            DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("yyyy-MM-dd'T'HH:mm:sszzz"),
            UseSimpleDictionaryFormat = true
        };

        public DataItemRestClient(IHttpClient client, string endpointPath)
        {
            this.EndpointPath = endpointPath;
            this.client = client;
        }

        public DataItemRestClient(IHttpClient client)
        {
            this.client = client;
        }

        public void Add(IEnumerable<T> items)
        {
            var json = this.Serialize(items);
            this.client.Post(this.EndpointPath, json);
        }

        public IEnumerable<T> GetAll()
        {
            var response = this.client.Get(this.EndpointPath);
            return this.Deserialize<List<T>>(response);
        }

        public void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.client.Delete(this.EndpointPath + $"/{item.Id}");
            }
        }

        public void Update(IEnumerable<T> items)
        {
            var json = this.Serialize(items);
            this.client.Put(this.EndpointPath, json);
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

        protected TDes Deserialize<TDes>(string str) where TDes : class
        {
            var serializer = new DataContractJsonSerializer(typeof(TDes), this.serializerSettings);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return serializer.ReadObject(stream) as TDes;
        }
    }
}