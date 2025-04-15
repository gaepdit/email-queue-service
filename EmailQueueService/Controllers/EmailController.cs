using EmailQueueService.Data;
using EmailQueueService.Models;
using EmailQueueService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailQueueService.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController(IQueueService queueService, EmailQueueDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetAllTasks()
    {
        var tasks = await dbContext.EmailTasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> EnqueueEmails([FromBody] EmailTask[] tasks)
    {
        await queueService.EnqueueItems(tasks);
        return Ok($"Enqueued {tasks.Length} email tasks for processing");
    }
}
