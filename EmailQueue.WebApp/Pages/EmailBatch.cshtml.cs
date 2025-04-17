using EmailQueue.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EmailQueue.WebApp.Pages;

public class EmailBatchModel(EmailQueueApiService apiService, ILogger<EmailBatchModel> logger) : PageModel
{
    [FromRoute]
    public Guid? Id { get; set; }

    [BindProperty]
    [Display(Name = "Batch ID")]
    public Guid? BatchId { get; set; }

    public IEnumerable<EmailTaskViewModel> EmailTasks { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? NotificationMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (Id == null) return;

        try
        {
            EmailTasks = await apiService.GetBatchEmailTasksAsync(Id.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching email tasks");
            ErrorMessage = "Error fetching email tasks. Please try again later.";
        }
    }

    public IActionResult OnPostAsync()
    {
        if (BatchId != null) return RedirectToPage(new { Id = BatchId });
        NotificationMessage = "Please enter a valid Batch ID to continue.";
        return RedirectToPage();
    }
}
