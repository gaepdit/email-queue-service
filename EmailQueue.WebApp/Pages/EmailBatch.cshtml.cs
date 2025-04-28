using EmailQueue.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EmailQueue.WebApp.Pages;

[Authorize]
public class EmailBatchModel(EmailQueueApiService apiService, ILogger<EmailBatchModel> logger) : PageModel
{
    [FromRoute]
    public string? Id { get; set; }

    [BindProperty]
    [Display(Name = "Batch ID")]
    public string? BatchId { get; set; }

    public IEnumerable<EmailTaskViewModel> EmailTasks { get; private set; } = [];
    public string? ErrorMessage { get; private set; }
    public bool ShowResults => !string.IsNullOrEmpty(Id) && ErrorMessage == null;

    [TempData]
    public string? NotificationMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrEmpty(Id)) return;

        try
        {
            EmailTasks = await apiService.GetBatchEmailTasksAsync(Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching emails for batch {BatchId}", Id);
            ErrorMessage = "Error fetching emails. Please try again later.";
        }
    }

    public IActionResult OnPostAsync()
    {
        if (BatchId != null) return RedirectToPage(new { Id = BatchId });
        NotificationMessage = "Please enter a valid Batch ID to continue.";
        return RedirectToPage("EmailBatch", new { Id = (Guid?)null });
    }
}
