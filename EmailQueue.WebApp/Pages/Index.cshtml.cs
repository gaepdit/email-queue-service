using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmailQueue.WebApp.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? RedirectToPage("/EmailBatch") : Challenge();
}
