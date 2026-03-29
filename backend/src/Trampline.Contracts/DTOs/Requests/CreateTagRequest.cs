namespace Trampline.Contracts.DTOs.Requests;

public record CreateTagRequest(string Name, string Category = "tech");
