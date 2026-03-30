namespace Trampline.Contracts.DTOs.Requests;

public class UpdatePrivacyRequest
{
    public bool? IsPrivate { get; set; }
    public bool? HideApplications { get; set; }
    public bool? HideResume { get; set; }
}
