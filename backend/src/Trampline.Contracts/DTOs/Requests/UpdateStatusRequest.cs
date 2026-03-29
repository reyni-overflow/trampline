using Trampline.Core.Models;

namespace Trampline.Contracts.DTOs.Requests;

public class UpdateStatusRequest
{
    public ApplicationStatus Status { get; set; }
}
