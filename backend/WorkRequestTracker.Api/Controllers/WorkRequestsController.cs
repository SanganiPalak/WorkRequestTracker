using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Data;
using WorkRequestTracker.Api.DTOs;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Controllers;

[ApiController]
[Route("api/work-requests")]
public class WorkRequestsController : ControllerBase
{
    private static readonly string[] Priorities = ["Low", "Medium", "High"];
    private static readonly string[] Statuses = ["New", "InProgress", "Blocked", "Completed"];
    private readonly WorkRequestDbContext _db;

    public WorkRequestsController(WorkRequestDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<WorkRequestListItemDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(Error("page must be at least 1 and pageSize must be between 1 and 100."));

        var query = _db.WorkRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Statuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(Error("Invalid status value."));
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.ClientName.Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new WorkRequestListItemDto
            {
                Id = x.Id, Title = x.Title, ClientName = x.ClientName,
                Priority = x.Priority, Status = x.Status, DueDate = x.DueDate,
                CreatedDate = x.CreatedDate, UpdatedDate = x.UpdatedDate
            }).ToListAsync();

        return Ok(new PagedResult<WorkRequestListItemDto>
        {
            Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkRequestDetailsDto>> GetById(int id)
    {
        var item = await _db.WorkRequests.AsNoTracking()
            .Include(x => x.Notes).FirstOrDefaultAsync(x => x.Id == id);

        if (item == null) return NotFound(Error($"Work request {id} was not found."));

        return Ok(new WorkRequestDetailsDto
        {
            Id = item.Id, Title = item.Title, ClientName = item.ClientName,
            Description = item.Description, Priority = item.Priority, Status = item.Status,
            DueDate = item.DueDate, CreatedDate = item.CreatedDate, UpdatedDate = item.UpdatedDate,
            Notes = item.Notes.OrderByDescending(n => n.CreatedDate).Select(n => new WorkRequestNoteDto
            {
                Id = n.Id, Text = n.Text, CreatedDate = n.CreatedDate
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<WorkRequestDetailsDto>> Create(CreateWorkRequestDto dto)
    {
        if (!Priorities.Contains(dto.Priority, StringComparer.OrdinalIgnoreCase))
            return BadRequest(Error("Priority must be Low, Medium, or High."));
        if (!Statuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(Error("Status must be New, InProgress, Blocked, or Completed."));
        if (dto.DueDate == null)
            return BadRequest(Error("DueDate is required."));

        var request = new WorkRequest
        {
            Title = dto.Title.Trim(), ClientName = dto.ClientName.Trim(),
            Description = dto.Description.Trim(),
            Priority = Normalize(dto.Priority, Priorities),
            Status = Normalize(dto.Status, Statuses),
            DueDate = dto.DueDate.Value,
            CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            request.Notes.Add(new WorkRequestNote { Text = dto.Notes.Trim(), CreatedDate = DateTime.UtcNow });

        _db.WorkRequests.Add(request);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = request.Id }, new WorkRequestDetailsDto
        {
            Id = request.Id, Title = request.Title, ClientName = request.ClientName,
            Description = request.Description, Priority = request.Priority, Status = request.Status,
            DueDate = request.DueDate, CreatedDate = request.CreatedDate, UpdatedDate = request.UpdatedDate,
            Notes = request.Notes.Select(n => new WorkRequestNoteDto
            { Id = n.Id, Text = n.Text, CreatedDate = n.CreatedDate }).ToList()
        });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        if (!Statuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(Error("Status must be New, InProgress, Blocked, or Completed."));

        var request = await _db.WorkRequests.FindAsync(id);
        if (request == null) return NotFound(Error($"Work request {id} was not found."));

        request.Status = Normalize(dto.Status, Statuses);
        request.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { request.Id, request.Status, request.UpdatedDate });
    }

    [HttpPost("{id:int}/notes")]
    public async Task<IActionResult> AddNote(int id, AddNoteDto dto)
    {
        var request = await _db.WorkRequests.FindAsync(id);
        if (request == null) return NotFound(Error($"Work request {id} was not found."));

        var note = new WorkRequestNote
        {
            WorkRequestId = id, Text = dto.Text.Trim(), CreatedDate = DateTime.UtcNow
        };
        _db.WorkRequestNotes.Add(note);
        request.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Created($"/api/work-requests/{id}", new
        {
            note.Id, note.WorkRequestId, note.Text, note.CreatedDate
        });
    }

    private static object Error(string message) => new
    {
        error = new { message, timestamp = DateTime.UtcNow }
    };

    private static string Normalize(string value, string[] allowed) =>
        allowed.First(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
}
