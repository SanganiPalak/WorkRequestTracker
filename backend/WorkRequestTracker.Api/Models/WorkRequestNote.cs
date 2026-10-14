namespace WorkRequestTracker.Api.Models;

public class WorkRequestNote
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public WorkRequest? WorkRequest { get; set; }
}
