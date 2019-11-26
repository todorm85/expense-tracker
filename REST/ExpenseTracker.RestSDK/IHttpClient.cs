namespace ExpenseTracker.RestClient
{
    public interface IHttpClient
    {
        void Post(string endpointPath, string json);

        string Get(string endpointPath);

        void Delete(string v);

        void Put(string endpointPath, string json);
    }
}