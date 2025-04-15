using EmailQueueService.Data;
using EmailQueueService.Models;
using EmailQueueService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace EmailQueueService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = nameof(SecuritySchemeType.ApiKey))]
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
        await queueService.EnqueueItems(tasks, User.Identity?.Name ?? "[unknown]");
        return Ok($"Enqueued {tasks.Length} email tasks for processing");
    }
}
