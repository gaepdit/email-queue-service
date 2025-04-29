using EmailQueue.WebApp.Platform;
using Microsoft.Extensions.Options;

namespace EmailQueue.WebApp.Services;

public class EmailQueueApiService(
    IHttpClientFactory httpClientFactory,
    IOptionsSnapshot<EmailQueueApi> apiSettings,
    ILogger<EmailQueueApiService> logger)
{
    public async Task<IEnumerable<EmailTaskViewModel>> GetBatchEmailTasksAsync(string batchId)
    {
        logger.LogInformation("Getting batch {BatchId}", batchId);
        return await GetApiDataAsync<EmailTaskViewModel>($"emailTasks/list/{batchId}");
    }

    public async Task<IEnumerable<BatchViewModel>> GetAllBatchesAsync()
    {
        logger.LogInformation("Getting all batches");
        return await GetApiDataAsync<BatchViewModel>("emailTasks/list");
    }

    private async Task<IEnumerable<T>> GetApiDataAsync<T>(string endpoint) where T : IEndPointViewModel
    {
        using var client = httpClientFactory.CreateClient(nameof(EmailQueueApiService));
        client.DefaultRequestHeaders.Add("X-API-Key", apiSettings.Value.ApiKey);
        using var response = await client.GetAsync(UriCombine(apiSettings.Value.BaseUrl, endpoint));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>().ConfigureAwait(false) ?? [];
    }

    private static Uri UriCombine(string baseUrl, string? endpoint)
    {
        var baseUri = new Uri(baseUrl);
        if (!baseUri.IsAbsoluteUri) throw new ArgumentOutOfRangeException(nameof(baseUrl));

        if (string.IsNullOrEmpty(endpoint)) return baseUri;

        const char separator = '/';
        return new Uri(baseUri.ToString().TrimEnd(separator) + separator + endpoint.TrimStart(separator));
    }
}

public record EmailTaskViewModel : IEndPointViewModel
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

public record BatchViewModel : IEndPointViewModel
{
    public required string BatchId { get; init; }
    public DateTime CreatedAt { get; [UsedImplicitly] init; }
}

public interface IEndPointViewModel;
