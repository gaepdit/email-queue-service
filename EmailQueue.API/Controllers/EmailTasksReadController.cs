using EmailQueue.API.AuthHandlers;
using EmailQueue.API.Data;
using EmailQueue.API.Models;
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

    [HttpGet("{batchId:maxlength(10)}")]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetBatchAsync([FromRoute] string batchId)
    {
        var tasks = await dbContext.EmailTasks
            .Where(t => t.BatchId.ToUpper() == batchId.ToUpper())
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }
}
