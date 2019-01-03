using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.RestApi
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration config;

        public BasicAuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            this.next = next;
            this.config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                
                var encodedBasicAuth = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                
                var decodedBasicAuth = Encoding.UTF8.GetString(Convert.FromBase64String(encodedBasicAuth));
                
                var client = decodedBasicAuth.Split(':', 2)[0];
                var secret = decodedBasicAuth.Split(':', 2)[1];
                
                if (this.IsAuthorized(client, secret))
                {
                    await this.next.Invoke(context);
                    return;
                }
            }
            
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        
        public bool IsAuthorized(string client, string secret)
        {
            var clientConfig = config.GetSection("clients").GetChildren().Select(x => new
            {
                client = x["client"],
                secret = x["secret"]
            });

            var match = clientConfig.FirstOrDefault(c => c.client == client);
            if (match != null && !string.IsNullOrEmpty(secret))
            {
                return match.secret == secret;
            }

            return false;
        }
    }
}