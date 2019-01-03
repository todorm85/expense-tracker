using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace ExpenseTracker.RestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });

            services.AddTransient<IUnitOfWork, UnitOfWork>(s =>
                new UnitOfWork(this.Configuration.GetConnectionString("LiteDbConnectionString")));
            services.AddTransient<IExpensesService, ExpensesService>();
            services.AddTransient<IBudgetService, BudgetService>();
            services.AddTransient<IBaseDataItemService<Category>, CategoriesService>();
            services.AddSingleton<ICustomLogger, SimpleFileLogger>(s => 
                new SimpleFileLogger(Configuration.GetSection("FileLoggerPath").Value));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<BasicAuthMiddleware>(this.Configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}