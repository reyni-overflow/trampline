namespace Trampline.Shared.Results;

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(
        T? value,
        bool isSuccess,
        IReadOnlyList<ErrorDetail> errors,
        int? errorCode = null)
        : base(isSuccess, errors, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) =>
        new(value, true, Array.Empty<ErrorDetail>());

    public new static Result<T> Failure(ErrorDetail error) =>
        new(default, false, new[] { error }, error.Code);

    public new static Result<T> Failure(params ErrorDetail[] errors) =>
        new(default, false, errors);

    public Result ToResult() =>
        IsSuccess ? Result.Success() : Result.Failure(Errors.ToArray());
}


public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public int? ErrorCode { get; }
    public IReadOnlyList<ErrorDetail> Errors { get; }

    protected Result(
        bool isSuccess,
        IReadOnlyList<ErrorDetail> errors,
        int? errorCode = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<ErrorDetail>();
        ErrorCode = errorCode ?? Errors.FirstOrDefault()?.Code;
    }

    public static Result Success() =>
        new(true, Array.Empty<ErrorDetail>());

    public static Result Failure(ErrorDetail error) =>
        new(false, new[] { error });

    public static Result Failure(params ErrorDetail[] errors) =>
        new(false, errors);
}