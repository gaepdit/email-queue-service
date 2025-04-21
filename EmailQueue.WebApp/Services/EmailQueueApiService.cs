using EmailQueue.WebApp.Platform;

namespace EmailQueue.WebApp.Services;

public class EmailQueueApiService(IHttpClientFactory httpClientFactory, ILogger<EmailQueueApiService> logger)
{
    public async Task<IEnumerable<EmailTaskViewModel>> GetBatchEmailTasksAsync(Guid batchId)
    {
        using var client = httpClientFactory.CreateClient(nameof(EmailQueueApiService));
        client.BaseAddress = new Uri(AppSettings.EmailQueueApi.BaseUrl);
        client.DefaultRequestHeaders.Add("X-API-Key", AppSettings.EmailQueueApi.ApiKey);

        using var response = await client.GetAsync($"emailTasks/list/{batchId}");
        response.EnsureSuccessStatusCode();

        var emailTasks = await response.Content.ReadFromJsonAsync<List<EmailTaskViewModel>>().ConfigureAwait(false);
        return emailTasks ?? [];
    }
}

public record EmailTaskViewModel
{
    public int Counter { get; init; }
    public required string Status { get; init; }
    public string? ApiKeyOwner { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? SentAt { get; init; }
    public List<string> Recipients { get; init; } = [];
    public required string Subject { get; init; }
}
