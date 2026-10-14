using System.ComponentModel.DataAnnotations;

namespace WorkRequestTracker.Api.DTOs;

public class UpdateStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
