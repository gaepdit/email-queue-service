using EmailQueue.API.AuthHandlers;
using EmailQueue.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace EmailQueue.API.Controllers;

[ApiController]
[Route("/")]
[Authorize(AuthenticationSchemes = nameof(SecuritySchemeType.ApiKey))]
public class EmailTasksReadController(EmailQueueDbContext dbContext) : ControllerBase
{
    [HttpGet("batches")]
    public async Task<ActionResult> GetAllBatchesAsync() =>
        Ok(await dbContext.EmailTasks
            .Where(t => t.ApiKeyOwner == User.ApiKeyOwner())
            .GroupBy(t => t.BatchId)
            .Select(g => new { BatchId = g.Key, Count = g.Count(), CreatedAt = g.Min(t => t.CreatedAt) })
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync());

    [HttpGet("batch/{batchId:maxlength(10)}")]
    [SuppressMessage("Performance",
        "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    public async Task<ActionResult> GetBatchAsync([FromRoute] string batchId) =>
        Ok(await dbContext.EmailTasks
            .Where(t => t.BatchId.ToUpper() == batchId.ToUpper() && t.ApiKeyOwner == User.ApiKeyOwner())
            .OrderBy(t => t.CreatedAt)
            .ToListAsync());
}
