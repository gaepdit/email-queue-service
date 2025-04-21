using EmailQueue.API.Models;
using EmailQueue.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace EmailQueue.API.Controllers;

[ApiController]
[Route("emailTasks")]
[Authorize(AuthenticationSchemes = nameof(SecuritySchemeType.ApiKey))]
[Authorize(nameof(PermissionRequirement.WritePermission))]
public class EmailTasksWriteController(IQueueService queueService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> EnqueueEmailsAsync([FromBody] EmailTask[] emailTasks)
    {
        var emptySubmissionResult = new { status = "Empty", message = "No email tasks submitted.", count = 0 };
        if (emailTasks.Length == 0) return Ok(emptySubmissionResult);

        var batchId = await queueService.EnqueueItems(emailTasks, User.Identity?.Name ?? "[unknown]");

        return batchId == null
            ? Ok(emptySubmissionResult)
            : Ok(new { status = "Success", message = "Emails have been queued.", count = emailTasks.Length, batchId });
    }
}
