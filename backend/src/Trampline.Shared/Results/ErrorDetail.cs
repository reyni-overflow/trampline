namespace Trampline.Shared.Results;

public record ErrorDetail(string Field, string Message, int? Code = null);