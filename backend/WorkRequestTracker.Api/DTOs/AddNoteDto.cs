using System.ComponentModel.DataAnnotations;

namespace WorkRequestTracker.Api.DTOs;

public class AddNoteDto
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;
}
