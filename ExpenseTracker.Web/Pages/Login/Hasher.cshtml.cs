using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Web.Pages.Login
{
    public class HasherModel : PageModel
    {
        public string Hashed { get; set; }
        public void OnGet()
        {
            PasswordHasher<IdentityUser> hasher = GetHasher();
            this.Hashed = hasher.HashPassword(null, "");
        }

        public static PasswordHasher<IdentityUser> GetHasher()
        {
            var hasherOptions = new PasswordHasherOptions() { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3 };
            var options = Options.Create(hasherOptions);
            var hasher = new PasswordHasher<IdentityUser>(options);
            return hasher;
        }
    }
}
