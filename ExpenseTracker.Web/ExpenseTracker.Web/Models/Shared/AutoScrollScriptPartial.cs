using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExpenseTracker.Web.Models.Shared
{
    public class AutoScrollScriptPartial
    {
        public static RouteValueDictionary AppendQueryParamsFromRequest(HttpRequest request, RouteValueDictionary query)
        {
            query.Add("XPosition", request.Query["XPosition"]);
            query.Add("YPosition", request.Query["YPosition"]);
            return query;
        }

        public static RouteValueDictionary GetQueryParamsFromRequest(HttpRequest request)
        {
            return AppendQueryParamsFromRequest(request, new RouteValueDictionary());
        }
    }
}
