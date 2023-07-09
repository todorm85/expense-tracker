using Microsoft.AspNetCore.Builder;

namespace ExpenseTracker.Web
{
    public static class ApplicationBuilderAuthentication
    {
        public static void AddLocalAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.Use(async (ctx, next) =>
             {
                 if (ctx.User.Identity.IsAuthenticated || ctx.Request.Path == "/login/hasher" || ctx.Request.Path == "/login")
                 {
                     await next.Invoke();
                 }
                 else
                 {
                     ctx.Response.Redirect("/login");
                 }
             });
        }
    }
}
