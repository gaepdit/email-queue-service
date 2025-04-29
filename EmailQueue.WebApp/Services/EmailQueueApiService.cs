using EmailQueue.WebApp.Platform;

namespace EmailQueue.WebApp.Services;

public class EmailQueueApiService(IHttpClientFactory httpClientFactory, ILogger<EmailQueueApiService> logger)
{
    public async Task<IEnumerable<EmailTaskViewModel>> GetBatchEmailTasksAsync(string batchId)
    {
        using var client = httpClientFactory.CreateClient(nameof(EmailQueueApiService));
        client.DefaultRequestHeaders.Add("X-API-Key", AppSettings.EmailQueueApi.ApiKey);
        logger.LogInformation("Getting batch {BatchId}", batchId);

        using var response = await client.GetAsync(UriCombine(AppSettings.EmailQueueApi.BaseUrl,
            relativeUri: $"emailTasks/list/{batchId}"));
        response.EnsureSuccessStatusCode();

        var emailTasks = await response.Content.ReadFromJsonAsync<List<EmailTaskViewModel>>().ConfigureAwait(false);
        return emailTasks ?? [];
    }

    private static Uri UriCombine(string baseUrl, string? relativeUri)
    {
        var baseUri = new Uri(baseUrl);
        if (!baseUri.IsAbsoluteUri) throw new ArgumentOutOfRangeException(nameof(baseUrl));

        if (string.IsNullOrEmpty(relativeUri)) return baseUri;

        const char separator = '/';
        return new Uri(baseUri.ToString().TrimEnd(separator) + separator + relativeUri.TrimStart(separator));
    }
}

public record EmailTaskViewModel
{
    public int Counter { get; [UsedImplicitly] init; }
    public required string Status { get; [UsedImplicitly] init; }
    public string? ApiKeyOwner { get; [UsedImplicitly] init; }
    public DateTime CreatedAt { get; [UsedImplicitly] init; }
    public DateTime? AttemptedAt { get; [UsedImplicitly] init; }
    [UsedImplicitly] public List<string> Recipients { get; init; } = [];
    public required string From { get; [UsedImplicitly] init; }
    public required string Subject { get; [UsedImplicitly] init; }
}
