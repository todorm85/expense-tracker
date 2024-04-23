using ExpenseTracker.App;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System;
using System.Runtime.InteropServices;
using Unity;

namespace ExpenseTracker.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            if (!env.IsDevelopment())
                app.AddLocalAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        public void ConfigureContainer(IUnityContainer container)
        {
            var expenseTrackerConfig = Configuration.GetSection("ExpenseTracker");
            var dbPath = expenseTrackerConfig.GetValue<string>("DatabasePath");
            if (dbPath.Contains("%"))
            {
                dbPath = Environment.ExpandEnvironmentVariables(dbPath);
            }

            Application.RegisterDependencies(container, new Config()
            {
                DbPath = dbPath,
                MailUser = Configuration["mailUser"],
                MailPass = Configuration["mailPass"],
                UserHashedPass = Configuration["userHashedPass"],
                LockoutMinutes = Configuration["LockoutMinutes"] != default ? int.Parse(Configuration["LockoutMinutes"]) : 0
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logger =>
            {
                logger.ClearProviders();
                logger.AddConsole();
                if (!env.IsDevelopment() && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    logger.AddEventLog(new EventLogSettings
                    {
                        SourceName = "Expenses",
                        LogName = "Expenses"
                    });
                }
            });
            services.AddSession();
            services.AddMemoryCache();
            var mvcBuilder = services.AddRazorPages();
            if (this.env.IsDevelopment())
                mvcBuilder.AddRazorRuntimeCompilation();

            mvcBuilder.AddRazorPagesOptions(o =>
            {
                o.Conventions.AddPageRoute("/Transactions/Upload", "");
            });

            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options => options.MaxModelBindingCollectionSize = 10000);
            services.Configure<FormOptions>(options => options.ValueCountLimit = 100000);
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            services.AddHttpClient();

            if (!env.IsDevelopment())
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        }
    }
}