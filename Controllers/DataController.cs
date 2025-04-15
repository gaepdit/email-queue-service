using Microsoft.AspNetCore.Mvc;
using EmailQueueService.Models;
using EmailQueueService.Services;

namespace EmailQueueService.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IQueueService queueService, ILogger<EmailController> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult EnqueueEmails([FromBody] List<EmailTask> tasks)
    {
        _queueService.EnqueueItems(tasks);
        return Ok($"Enqueued {tasks.Count} email tasks for processing");
    }
}