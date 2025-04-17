namespace EmailQueue.WebApp.Services;

public class EmailQueueApiService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<EmailQueueApiService> logger)
{
    private readonly string _baseUrl = configuration["EmailQueueApi:BaseUrl"] ?? "https://localhost:7145";
    private readonly string _apiKey = configuration["EmailQueueApi:ApiKey"] ?? "your-secret-api-key-1";

    public async Task<IEnumerable<EmailTaskViewModel>> GetBatchEmailTasksAsync(Guid batchId)
    {
        try
        {
            using var client = httpClientFactory.CreateClient(nameof(EmailQueueApiService));
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);

            using var response = await client.GetAsync($"emailTasks/list/{batchId}");
            response.EnsureSuccessStatusCode();

            var emails = await response.Content.ReadFromJsonAsync<List<EmailTaskViewModel>>().ConfigureAwait(false);
            return emails ?? Enumerable.Empty<EmailTaskViewModel>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching email tasks for batch {BatchId}", batchId);
            throw;
        }
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
