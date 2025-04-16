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
public class EmailTasksController(IQueueService queueService, EmailQueueDbContext dbContext) : ControllerBase
{
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetAllAsync()
    {
        var tasks = await dbContext.EmailTasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("list/{batchId:guid}")]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetBatchAsync([FromRoute] Guid batchId)
    {
        var tasks = await dbContext.EmailTasks
            .Where(t => t.BatchId == batchId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> EnqueueEmailsAsync([FromBody] EmailTask[] emailTasks)
    {
        var emptySubmissionResult = new { message = "No email tasks submitted.", count = 0 };
        if (emailTasks.Length == 0) return Ok(emptySubmissionResult);

        var batchId = await queueService.EnqueueItems(emailTasks, User.Identity?.Name ?? "[unknown]");
        return batchId == null
            ? Ok(emptySubmissionResult)
            : Ok(new { message = "Emails have been queued.", count = emailTasks.Length, batchId });
    }
}
