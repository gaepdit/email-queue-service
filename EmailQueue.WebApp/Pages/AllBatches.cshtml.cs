using EmailQueue.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmailQueue.WebApp.Pages;

[Authorize]
public class AllBatchesModel(EmailQueueApiService apiService, ILogger<EmailBatchModel> logger) : PageModel
{
    public IEnumerable<BatchViewModel> AllBatches { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            AllBatches = await apiService.GetAllBatchesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching batches");
            ErrorMessage = "Error fetching data. Please try again later.";
        }
    }
}
