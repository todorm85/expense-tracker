using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.App;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddRazorPages();
            if (this.env.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
                mvcBuilder.AddRazorPagesOptions(o =>
                {
                    o.Conventions.AddPageRoute("/Transactions/List", "");
                });
            }

            services.Configure<FormOptions>(options => options.ValueCountLimit = 4096);
        }

        public void ConfigureContainer(IUnityContainer container)
        {
            var expenseTrackerConfig = Configuration.GetSection("ExpenseTracker");
            var dbPath = expenseTrackerConfig.GetValue<string>("DatabasePath");
            Application.RegisterDependencies(container, new Config() { DbPath = dbPath });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
