using System.ComponentModel.DataAnnotations;

namespace WorkRequestTracker.Api.DTOs;

public class CreateWorkRequestDto
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(120, MinimumLength = 2)]
    public string ClientName { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "Medium";

    [Required]
    public string Status { get; set; } = "New";

    [Required]
    public DateTime? DueDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
