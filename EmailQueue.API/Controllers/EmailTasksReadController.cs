using EmailQueue.API.AuthHandlers;
using EmailQueue.API.Data;
using EmailQueue.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace EmailQueue.API.Controllers;

[ApiController]
[Route("emailTasks/list")]
[Authorize(AuthenticationSchemes = nameof(SecuritySchemeType.ApiKey))]
public class EmailTasksReadController(EmailQueueDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetAllBatchesAsync() =>
        Ok(await dbContext.EmailTasks
            .Where(t => t.ApiKeyOwner == User.ApiKeyOwner())
            .GroupBy(t => t.BatchId)
            .Select(g => new { BatchId = g.Key, CreatedAt = g.Min(t => t.CreatedAt) })
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync());

    [HttpGet("{batchId:maxlength(10)}")]
    [SuppressMessage("Performance",
        "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    public async Task<ActionResult<IEnumerable<EmailTask>>> GetBatchAsync([FromRoute] string batchId) =>
        Ok(await dbContext.EmailTasks
            .Where(t => t.BatchId.ToUpper() == batchId.ToUpper() && t.ApiKeyOwner == User.ApiKeyOwner())
            .OrderBy(t => t.CreatedAt)
            .ToListAsync());
}
