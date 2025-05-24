using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Runtime.InteropServices;

namespace ExpenseTracker.Web
{
    public static class ApplicationBuilderExtensions
    {
        public static void AddLocalAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.Use(async (ctx, next) =>
             {
                 if ((ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated) || ctx.Request.Path == "/login/hasher" || ctx.Request.Path == "/login")
                 {
                     await next.Invoke();
                 }
                 else
                 {
                     ctx.Response.Redirect("/login");
                 }
             });
        }

        public static IServiceCollection AddExpenseTrackerLogging(this IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    loggingBuilder.AddEventLog(new EventLogSettings
                    {
                        SourceName = "Expenses",
                        LogName = "Expenses"
                    });
                }
            });
            
            return services;
        }
    }
}
