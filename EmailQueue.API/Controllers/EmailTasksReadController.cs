using EmailQueue.API.Data;
using EmailQueue.API.Models;
using EmailQueue.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace EmailQueue.API.Controllers;

[ApiController]
[Route("emailTasks/list")]
[Authorize(AuthenticationSchemes = nameof(SecuritySchemeType.ApiKey))]
[Authorize(nameof(PermissionRequirement.ReadPermission))]
public class EmailTasksReadController(EmailQueueDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetAllAsync()
    {
        var tasks = await dbContext.EmailTasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{batchId:guid}")]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetBatchAsync([FromRoute] Guid batchId)
    {
        var tasks = await dbContext.EmailTasks
            .Where(t => t.BatchId == batchId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }
}
