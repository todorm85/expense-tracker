using ExpenseTracker.App;
using ExpenseTracker.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var env = builder.Environment;
var services = builder.Services;

Config appConfig = GetApplicationConfiguration(env.EnvironmentName);
Application.RegisterDependencies(services, appConfig);

if (env.IsProduction())
    services.AddExpenseTrackerLogging(env);

services.AddSession();
services.AddMemoryCache();

var mvcBuilder = services.AddRazorPages();
if (env.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

mvcBuilder.AddRazorPagesOptions(o =>
{
    o.Conventions.AddPageRoute("/Transactions/Upload", "");
}).AddSessionStateTempDataProvider();

services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options => options.MaxModelBindingCollectionSize = 10000);
services.Configure<FormOptions>(options => options.ValueCountLimit = 100000);
services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
services.AddHttpClient();

bool enableAuthentication = configuration.GetValue<bool>("EnableAuthentication");
if (enableAuthentication)
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

// Configure pipeline
var app = builder.Build();

if (env.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseStaticFiles();

app.UseSession();

app.UseRouting();
if (enableAuthentication)
    app.AddLocalAuthentication();

app.MapRazorPages();

app.Run();

Config GetApplicationConfiguration(string environmentName)
{
    var expenseTrackerConfig = configuration.GetSection("ExpenseTracker");
    var dbPath = expenseTrackerConfig.GetValue<string>("DatabasePath");
    if (!string.IsNullOrEmpty(dbPath) && dbPath.Contains("%"))
    {
        dbPath = Environment.ExpandEnvironmentVariables(dbPath);
    }

    int.TryParse(configuration["LockoutMinutes"], out int lockoutMinutes);
    var config = new Config()
    {
        DbPath = dbPath,
        MailUser = configuration["mailUser"],
        MailPass = configuration["mailPass"],
        UserHashedPass = configuration["userHashedPass"],
        LockoutMinutes = lockoutMinutes
    };

    return config;
}