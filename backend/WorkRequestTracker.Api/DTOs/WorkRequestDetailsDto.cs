namespace WorkRequestTracker.Api.DTOs;

public class WorkRequestDetailsDto : WorkRequestListItemDto
{
    public string Description { get; set; } = string.Empty;
    public List<WorkRequestNoteDto> Notes { get; set; } = new();
}

public class WorkRequestNoteDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
