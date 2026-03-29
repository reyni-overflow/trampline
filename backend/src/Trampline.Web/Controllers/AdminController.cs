using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trampline.Application.Services;
using Trampline.Application.Services.Employees;
using Trampline.Application.Services.Mentorships;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.Extensions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Repositories;

namespace Trampline.Web.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("admin")]
public class AdminController(
    ILogger<AdminController> logger,
    IUserService userService,
    IEmployeeRepository employeeRepository,
    IJobRepository jobRepository,
    IEventRepository eventRepository,
    IWorkerService workerService,
    IEmployeeService employeeService,
    INotificationService notificationService,
    ITagRepository tagRepository,
    IAuditService auditService,
    IAuditLogRepository auditLogRepository,
    IDaDataService daDataService,
    IMentorshipRepository mentorshipRepository,
    IMentorshipService mentorshipService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsersAsync(CancellationToken ct, int pageSize = 20, int pageNumber = 1)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(pageNumber, 1);
        var (users, total) = await userService.GetPaginatedAsync(pageNumber, pageSize, ct);
        var list = users.Select(u => new
        {
            u.Id,
            u.Nickname,
            u.Email,
            Role = u.Role.ToString(),
            u.Avatar,
            u.IsBlocked,
            u.IsSuperAdmin,
            CreatedAt = u.Sessions.MinBy(s => s.CreatedAt)?.CreatedAt
        });

        return Ok(new { items = list, total });
    }

    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLogsAsync(
        CancellationToken ct,
        int pageNumber = 1,
        int pageSize = 20,
        string? action = null,
        string? entityType = null,
        Guid? userId = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(pageNumber, 1);
        var (items, total) = await auditLogRepository.GetPaginatedAsync(pageNumber, pageSize, action, entityType, userId, ct);

        return Ok(new { items, total });
    }

    [HttpPut("users/{id:guid}/block")]
    public async Task<IActionResult> BlockUserAsync(Guid id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        if (user == null) return NotFound();

        if (user.Role == Role.Admin)
        {
            var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerIdStr == null) return Unauthorized();
            var caller = await userService.GetByIdAsync(Guid.Parse(callerIdStr), ct);
            if (caller == null || !caller.IsSuperAdmin)
                return StatusCode(403, new ProblemDetails { Title = "Only the super administrator can block admin users", Status = 403 });
        }

        user.Block();

        await userService.UpdateAsync(user, ct);
        logger.LogWarning("Admin blocked user {TargetUserId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "block_user", "User", id, $"Blocked user {user.Nickname}", ip, ct);

        return Ok();
    }

    [HttpPut("users/{id:guid}/unblock")]
    public async Task<IActionResult> UnblockUserAsync(Guid id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        if (user == null) return NotFound();

        if (user.Role == Role.Admin)
        {
            var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerIdStr == null) return Unauthorized();
            var caller = await userService.GetByIdAsync(Guid.Parse(callerIdStr), ct);
            if (caller == null || !caller.IsSuperAdmin)
                return StatusCode(403, new ProblemDetails { Title = "Only the super administrator can unblock admin users", Status = 403 });
        }

        user.Unblock();

        await userService.UpdateAsync(user, ct);
        logger.LogWarning("Admin unblocked user {TargetUserId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "unblock_user", "User", id, $"Unblocked user {user.Nickname}", ip, ct);

        return Ok();
    }

    public record ChangeRoleRequest(string Role);

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> ChangeRoleAsync(Guid id, [FromBody] ChangeRoleRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<Role>(request.Role, true, out var role))
            return BadRequest("Invalid role");

        var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (callerIdStr == null) return Unauthorized();
        var caller = await userService.GetByIdAsync(Guid.Parse(callerIdStr), ct);
        if (caller == null) return Unauthorized();

        if (role == Role.Admin && !caller.IsSuperAdmin)
            return StatusCode(403, new ProblemDetails { Title = "Only the super administrator can assign the Admin role", Status = 403 });

        var user = await userService.GetByIdAsync(id, ct);
        if (user == null) return NotFound();

        if (user.Role == Role.Admin && !caller.IsSuperAdmin)
            return StatusCode(403, new ProblemDetails { Title = "Only the super administrator can modify admin users", Status = 403 });

        var oldRole = user.Role.ToString();
        user.ChangeRole(role);

        await userService.UpdateAsync(user, ct);
        logger.LogWarning("Admin changed role of {TargetUserId} to {Role}", id, role);

        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(caller.Id, adminName, "Admin", "change_role", "User", id, $"Changed role from {oldRole} to {role}", ip, ct);

        return Ok();
    }

    [HttpGet("verification/pending")]
    public async Task<IActionResult> GetPendingVerificationsAsync(CancellationToken ct)
    {
        var unverified = await employeeRepository.GetUnverifiedAsync(ct);
        var profiles = unverified.Select(e => new
        {
            e.Id,
            CompanyName = e.Name,
            e.Info.INN,
            e.Info.Email,
            e.Link,
            e.Activity,
            e.VerificationLevel,
            CreatedAt = e.User.Sessions.Min(s => (DateTime?)s.CreatedAt)
        });

        return Ok(profiles);
    }

    [HttpGet("verification/check-inn/{inn}")]
    public async Task<IActionResult> CheckInnAsync(string inn, CancellationToken ct)
    {
        var result = await daDataService.FindParty(inn, ct);
        if (result.IsFailure)
            return Ok(new { found = false });

        var data = result.Value!;
        return Ok(new { found = true, data.Value, data.Inn, data.Kpp, orgn = data.ORGN, data.Type });
    }

    [HttpPut("verification/{id:guid}/approve")]
    public async Task<IActionResult> ApproveVerificationAsync(Guid id, CancellationToken ct)
    {
        var profile = await employeeRepository.GetByIdAsync(id, ct);
        if (profile == null) return NotFound();

        profile.SetVerificationLevel(2);

        await employeeRepository.UpdateAsync(profile, ct);
        logger.LogInformation("Admin approved verification {ProfileId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "approve_verification", "EmployeeProfile", id, $"Approved verification for {profile.Name}", ip, ct);

        try
        {
            await notificationService.SendAsync(profile.UserId, "verification_status", new
            {
                profileId = id,
                companyName = profile.Name,
                status = "approved"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send verification notification");
        }

        return Ok();
    }

    public record RejectRequest(string? Reason);

    [HttpPut("verification/{id:guid}/reject")]
    public async Task<IActionResult> RejectVerificationAsync(Guid id, [FromBody] RejectRequest request, CancellationToken ct)
    {
        var profile = await employeeRepository.GetByIdAsync(id, ct);
        if (profile == null) return NotFound();

        profile.SetVerificationLevel(0);

        await employeeRepository.UpdateAsync(profile, ct);
        logger.LogInformation("Admin rejected verification {ProfileId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "reject_verification", "EmployeeProfile", id, $"Rejected verification for {profile.Name}, reason: {request.Reason}", ip, ct);

        try
        {
            await notificationService.SendAsync(profile.UserId, "verification_status", new
            {
                profileId = id,
                companyName = profile.Name,
                status = "rejected",
                reason = request.Reason
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send verification notification");
        }

        return Ok();
    }

    [HttpGet("moderation/jobs")]
    public async Task<IActionResult> GetJobsForModerationAsync(CancellationToken ct)
    {
        var pending = await jobRepository.GetPendingModerationAsync(ct);
        var jobs = pending.Select(j => j.ToJobResponse()).ToList();

        return Ok(jobs);
    }

    [HttpPut("moderation/jobs/{id:guid}/approve")]
    public async Task<IActionResult> ApproveJobAsync(Guid id, CancellationToken ct)
    {
        var job = await jobRepository.GetByIdAsync(id, ct);
        if (job == null) return NotFound();

        job.SetActive(true);

        await jobRepository.UpdateAsync(job, ct);
        logger.LogInformation("Admin approved job {JobId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "approve_job", "Job", id, $"Approved job: {job.Title}", ip, ct);

        try
        {
            await notificationService.SendAsync(job.UserId, "job_moderation", new
            {
                jobId = id,
                jobTitle = job.Title,
                status = "approved"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send job moderation notification");
        }

        return Ok();
    }

    [HttpPut("moderation/jobs/{id:guid}/reject")]
    public async Task<IActionResult> RejectJobAsync(Guid id, CancellationToken ct)
    {
        var job = await jobRepository.GetByIdAsync(id, ct);
        if (job == null) return NotFound();

        var jobUserId = job.UserId;
        var jobTitle = job.Title;

        await jobRepository.DeleteAsync(id, ct);
        logger.LogInformation("Admin rejected job {JobId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "reject_job", "Job", id, $"Rejected job: {jobTitle}", ip, ct);

        try
        {
            await notificationService.SendAsync(jobUserId, "job_moderation", new
            {
                jobId = id,
                jobTitle,
                status = "rejected"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send job moderation notification");
        }

        return Ok();
    }

    [HttpGet("moderation/events")]
    public async Task<IActionResult> GetEventsForModerationAsync(CancellationToken ct)
    {
        var pending = await eventRepository.GetPendingModerationAsync(ct);
        var events = pending.Select(e => e.ToEventResponse()).ToList();

        return Ok(events);
    }

    [HttpPut("moderation/events/{id:guid}/approve")]
    public async Task<IActionResult> ApproveEventAsync(Guid id, CancellationToken ct)
    {
        var evt = await eventRepository.GetByIdAsync(id, ct);
        if (evt == null) return NotFound();

        evt.SetActive(true);

        await eventRepository.UpdateAsync(evt, ct);
        logger.LogInformation("Admin approved event {EventId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "approve_event", "Event", id, $"Approved event: {evt.Title}", ip, ct);

        try
        {
            await notificationService.SendAsync(evt.UserId, "event_moderation", new
            {
                eventId = id,
                eventTitle = evt.Title,
                status = "approved"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send event moderation notification");
        }

        return Ok();
    }

    [HttpPut("moderation/events/{id:guid}/reject")]
    public async Task<IActionResult> RejectEventAsync(Guid id, CancellationToken ct)
    {
        var evt = await eventRepository.GetByIdAsync(id, ct);
        if (evt == null) return NotFound();

        var evtUserId = evt.UserId;
        var evtTitle = evt.Title;

        await eventRepository.DeleteAsync(id, ct);
        logger.LogInformation("Admin rejected event {EventId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "reject_event", "Event", id, $"Rejected event: {evtTitle}", ip, ct);

        try
        {
            await notificationService.SendAsync(evtUserId, "event_moderation", new
            {
                eventId = id,
                eventTitle = evtTitle,
                status = "rejected"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send event moderation notification");
        }

        return Ok();
    }

    [HttpGet("moderation/mentorships")]
    public async Task<IActionResult> GetMentorshipsForModerationAsync(CancellationToken ct)
    {
        var pending = await mentorshipRepository.GetPendingModerationAsync(ct);
        var mentorships = pending.Select(m => m.ToMentorshipResponse()).ToList();

        return Ok(mentorships);
    }

    [HttpPut("moderation/mentorships/{id:guid}/approve")]
    public async Task<IActionResult> ApproveMentorshipAsync(Guid id, CancellationToken ct)
    {
        var mentorship = await mentorshipRepository.GetByIdAsync(id, ct);
        if (mentorship == null) return NotFound();

        mentorship.SetActive(true);

        await mentorshipRepository.UpdateAsync(mentorship, ct);
        logger.LogInformation("Admin approved mentorship {MentorshipId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "approve_mentorship", "Mentorship", id, $"Approved mentorship: {mentorship.Title}", ip, ct);

        try
        {
            await notificationService.SendAsync(mentorship.UserId, "mentorship_moderation", new
            {
                mentorshipId = id,
                mentorshipTitle = mentorship.Title,
                status = "approved"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send mentorship moderation notification");
        }

        return Ok();
    }

    [HttpPut("moderation/mentorships/{id:guid}/reject")]
    public async Task<IActionResult> RejectMentorshipAsync(Guid id, CancellationToken ct)
    {
        var mentorship = await mentorshipRepository.GetByIdAsync(id, ct);
        if (mentorship == null) return NotFound();

        var mentorshipUserId = mentorship.UserId;
        var mentorshipTitle = mentorship.Title;

        await mentorshipRepository.DeleteAsync(id, ct);
        logger.LogInformation("Admin rejected mentorship {MentorshipId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "reject_mentorship", "Mentorship", id, $"Rejected mentorship: {mentorshipTitle}", ip, ct);

        try
        {
            await notificationService.SendAsync(mentorshipUserId, "mentorship_moderation", new
            {
                mentorshipId = id,
                mentorshipTitle,
                status = "rejected"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send mentorship moderation notification");
        }

        return Ok();
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetTagsAsync(CancellationToken ct)
    {
        var allTags = await tagRepository.GetAllWithUsageAsync(ct);
        var tags = allTags.Select(t => new
        {
            t.Name,
            Count = t.Jobs != null ? t.Jobs.Count : 0
        });

        return Ok(tags);
    }

    [HttpPost("tags")]
    public async Task<IActionResult> CreateTagAsync([FromBody] CreateTagRequest request, CancellationToken ct)
    {
        var exists = await tagRepository.ExistsAsync(request.Name, ct);
        if (exists) return Conflict("Tag already exists");

        var trimmedName = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName) || trimmedName.All(c => char.IsWhiteSpace(c) || char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format))
            return BadRequest(new ProblemDetails { Title = "Tag name must contain visible characters", Status = 400 });

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Category = request.Category,
            Lvl = 0
        };

        await tagRepository.AddAsync(tag, ct);

        logger.LogInformation("Tag created: {TagName}", tag.Name);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "create_tag", "Tag", tag.Id, $"Created tag: {tag.Name}", ip, ct);

        return Ok(tag);
    }

    [HttpDelete("tags/{name}")]
    public async Task<IActionResult> DeleteTagAsync(string name, CancellationToken ct)
    {
        await tagRepository.DeleteByNameAsync(name, ct);

        logger.LogInformation("Tag deleted: {TagName}", name);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "delete_tag", "Tag", null, $"Deleted tag: {name}", ip, ct);

        return Ok();
    }

    [HttpGet("curators")]
    public async Task<IActionResult> GetCuratorsAsync(CancellationToken ct)
    {
        var users = await userService.GetByRoleAsync(Role.Admin, ct);
        var admins = users.Select(u => new
        {
            u.Id,
            u.Nickname,
            u.Email,
            Role = u.Role.ToString(),
            u.Avatar,
            u.IsSuperAdmin
        });

        return Ok(admins);
    }

    public record CreateCuratorRequest(string Name, string Email, string Password);

    [HttpPost("curators")]
    public async Task<IActionResult> CreateCuratorAsync([FromBody] CreateCuratorRequest request, CancellationToken ct)
    {
        var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (callerIdStr == null) return Unauthorized();
        var caller = await userService.GetByIdAsync(Guid.Parse(callerIdStr), ct);
        if (caller == null || !caller.IsSuperAdmin)
            return StatusCode(403, new ProblemDetails { Title = "Only the super administrator can create curators", Status = 403 });

        var existing = await userService.GetByEmailAsync(request.Email, ct);
        if (existing != null) return Conflict("User with this email already exists");

        if (request.Password.Length < 8)
            return BadRequest(new ProblemDetails { Title = "Password must be at least 8 characters", Status = 400 });
        if (!request.Password.Any(char.IsUpper))
            return BadRequest(new ProblemDetails { Title = "Password must contain at least one uppercase letter", Status = 400 });
        if (!request.Password.Any(char.IsLower))
            return BadRequest(new ProblemDetails { Title = "Password must contain at least one lowercase letter", Status = 400 });
        if (!request.Password.Any(char.IsDigit))
            return BadRequest(new ProblemDetails { Title = "Password must contain at least one digit", Status = 400 });

        var result = await userService.CreateUserAsync(
            new RegisterRequest { Name = request.Name, Email = request.Email, Password = request.Password, Role = Role.Admin },
            ct
        );

        if (result.IsFailure)
            return BadRequest(result.Errors);

        logger.LogWarning("Admin created curator {Email}", request.Email);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "create_curator", "User", result.Value!.Id, $"Created curator: {request.Email}", ip, ct);

        return Ok(new { result.Value!.Id, result.Value!.Nickname, result.Value!.Email, Role = "Admin" });
    }

    [HttpDelete("curators/{id:guid}")]
    public async Task<IActionResult> DeleteCuratorAsync(Guid id, CancellationToken ct)
    {
        var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (callerIdStr == null) return Unauthorized();
        var caller = await userService.GetByIdAsync(Guid.Parse(callerIdStr), ct);
        if (caller == null || !caller.IsSuperAdmin)
            return StatusCode(403, new ProblemDetails { Title = "Only the super administrator can delete curators", Status = 403 });

        var user = await userService.GetByIdAsync(id, ct);

        if (user == null) return NotFound();
        if (user.Role != Role.Admin) return BadRequest("User is not a curator");
        if (user.IsSuperAdmin) return BadRequest("Cannot delete the super administrator");

        await userService.DeleteAsync(id, ct);
        logger.LogWarning("Admin deleted curator {CuratorId}", id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        await auditService.LogAsync(adminId is not null ? Guid.Parse(adminId) : null, adminName, "Admin", "delete_curator", "User", id, $"Deleted curator: {id}", ip, ct);

        return Ok();
    }

    [HttpPut("jobs/{id:guid}")]
    public async Task<IActionResult> UpdateJobAsync(Guid id, [FromBody] UpdateJobRequest request, CancellationToken ct)
    {
        var job = await jobRepository.GetByIdAsync(id, ct);
        if (job == null) return NotFound();

        job.Update(request.Title, request.Description, request.Address, request.City, request.Country,
            request.IsActive);
        job.UpdateSalary(request.SalaryFrom, request.SalaryTo);

        if (request.Tags.Length > 0)
        {
            var tags = await jobRepository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), ct);
            job.UpdateTags(tags.ToArray());
        }

        await jobRepository.UpdateAsync(job, ct);
        logger.LogWarning("Admin updated job {JobId}", id);
        return Ok(job.ToJobResponse());
    }

    [HttpPut("events/{id:guid}")]
    public async Task<IActionResult> UpdateEventAsync(Guid id, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        var evt = await eventRepository.GetByIdAsync(id, ct);
        if (evt == null) return NotFound();

        evt.Update(request.Title, request.Description, request.Address, request.City, request.Country,
            request.IsActive);
        evt.UpdateSalary(request.SalaryFrom, request.SalaryTo);

        await eventRepository.UpdateAsync(evt, ct);
        logger.LogWarning("Admin updated event {EventId}", id);
        return Ok(evt.ToEventResponse());
    }

    [HttpPut("mentorships/{id:guid}")]
    public async Task<IActionResult> UpdateMentorshipAsync(Guid id, [FromBody] UpdateMentorshipRequest request, CancellationToken ct)
    {
        var mentorship = await mentorshipRepository.GetByIdAsync(id, ct);
        if (mentorship == null) return NotFound();

        mentorship.Update(request.Title, request.Description, request.Address, request.City, request.Country,
            request.IsActive);
        mentorship.UpdateSalary(request.SalaryFrom, request.SalaryTo);

        await mentorshipRepository.UpdateAsync(mentorship, ct);
        logger.LogWarning("Admin updated mentorship {MentorshipId}", id);
        return Ok(mentorship.ToMentorshipResponse());
    }

    [HttpPut("workers/{userId:guid}")]
    public async Task<IActionResult> UpdateWorkerProfileAsync(Guid userId, [FromBody] WorkerProfileRequest request, CancellationToken ct)
    {
        var result = await workerService.UpdateProfileAsync(userId, request, ct);

        if (result.IsFailure)
            return BadRequest(result.Errors);

        logger.LogWarning("Admin updated worker profile for {UserId}", userId);
        return Ok();
    }

    [HttpPut("employees/{userId:guid}")]
    public async Task<IActionResult> UpdateEmployeeProfileAsync(Guid userId, [FromBody] EmployeeProfileRequest request, CancellationToken ct)
    {
        var result = await employeeService.UpdateProfileAsync(userId, request, ct);

        if (result.IsFailure)
            return BadRequest(result.Errors);

        logger.LogWarning("Admin updated employee profile for {UserId}", userId);
        return Ok();
    }
}
