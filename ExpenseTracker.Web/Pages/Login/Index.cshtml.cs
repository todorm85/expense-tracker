using System;
using System.Security.Claims;
using ExpenseTracker.App;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Web.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly Config config;
        private readonly ILogger logger;
        private static int failedAttempts = 0;
        private static DateTime lastAttempt = DateTime.MinValue;

        public IndexModel(Config config, ILogger<IndexModel> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        [BindProperty]
        public string Password { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var hasher = HasherModel.GetHasher();
            if (failedAttempts > 3)
            {
                if (DateTime.Now - lastAttempt < TimeSpan.FromMinutes(1))
                {
                    Message = $"Locked out. Wait {config.LockoutMinutes} minutes.";
                    if (failedAttempts == 4)
                    {
                        logger.LogCritical($"Attempted brute force login from ${HttpContext.Connection.RemoteIpAddress}");
                        failedAttempts++;
                    }

                    return Page();
                }
                else
                {
                    failedAttempts = 0;
                    Message = null;
                }
            }

            if (Password != null && hasher.VerifyHashedPassword(null, config.UserHashedPass, Password) == PasswordVerificationResult.Success)
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("Name", "Todor") }, CookieAuthenticationDefaults.AuthenticationScheme));
                this.HttpContext.SignInAsync(principal).Wait();
                return Redirect("/");
            }
            else
            {
                failedAttempts++;
                lastAttempt = DateTime.Now;
                return RedirectToPage();
            }
        }
    }
}
