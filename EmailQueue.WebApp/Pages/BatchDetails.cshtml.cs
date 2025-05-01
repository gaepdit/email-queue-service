using EmailQueue.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EmailQueue.WebApp.Pages;

[Authorize]
public class BatchDetailsModel(EmailQueueApiService apiService, ILogger<BatchDetailsModel> logger) : PageModel
{
    [BindProperty]
    [Display(Name = "Batch ID")]
    public string? BatchId { get; set; }

    public IEnumerable<EmailTaskViewModel> EmailTasks { get; private set; } = [];
    public string? ErrorMessage { get; private set; }
    public bool ShowResults { get; private set; }

    [TempData]
    public string? NotificationMessage { get; set; }

    public void OnGet()
    {
        // Method intentionally left empty.
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(BatchId) || BatchId.Length > 10)
        {
            NotificationMessage = "Please enter a valid Batch ID to continue.";
            return RedirectToPage();
        }

        try
        {
            EmailTasks = await apiService.GetBatchDetailsAsync(BatchId);
            ShowResults = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching emails for batch {BatchId}", BatchId);
            ErrorMessage = "Error fetching emails. Please try again later.";
        }

        return Page();
    }
}
