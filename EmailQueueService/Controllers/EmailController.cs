using Microsoft.AspNetCore.Mvc;
using EmailQueueService.Models;
using EmailQueueService.Services;
using EmailQueueService.Data;
using Microsoft.EntityFrameworkCore;

namespace EmailQueueService.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly ILogger<EmailController> _logger;
    private readonly EmailQueueDbContext _dbContext;

    public EmailController(
        IQueueService queueService,
        ILogger<EmailController> logger,
        EmailQueueDbContext dbContext)
    {
        _queueService = queueService;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetAllTasks()
    {
        var tasks = await _dbContext.EmailTasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
            
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> EnqueueEmails([FromBody] EmailTask[] tasks)
    {
        await _queueService.EnqueueItems(tasks);
        return Ok($"Enqueued {tasks.Length} email tasks for processing");
    }
}