using System.Text.Json.Serialization;
using Trampline.Shared.Results;

namespace Trampline.Core.Models.Worker;

public record WorkerInfo
{
    public string University { get; init; }

    public int Course { get; init; }

    public DateTime AdmissionAt { get; init; }

    public DateTime GraduationAt { get; init; }

    [JsonConstructor]
    public WorkerInfo(string university, int course, DateTime admissionAt, DateTime graduationAt)
    {
        University = university;
        Course = course;
        AdmissionAt = admissionAt;
        GraduationAt = graduationAt;
    }

    public static Result<WorkerInfo> Create(
        string university,
        int course,
        DateTime admissionAt,
        DateTime graduationAt)
    {
        if (string.IsNullOrWhiteSpace(university))
            return Result<WorkerInfo>.Failure(new ErrorDetail(nameof(university), "Университет обязателен", 400));

        if (course <= 0)
            return Result<WorkerInfo>.Failure(new ErrorDetail(nameof(course), "Курс должен быть больше 0", 400));

        if (course > 6)
            return Result<WorkerInfo>.Failure(new ErrorDetail(nameof(course), "Курс должен быть от 1 до 6", 400));

        if (graduationAt < admissionAt)
            return Result<WorkerInfo>.Failure(new ErrorDetail(nameof(graduationAt),
                "Дата окончания должна быть позже даты поступления", 400));

        return Result<WorkerInfo>.Success(new WorkerInfo(
            university.Trim(),
            course,
            admissionAt,
            graduationAt));
    }
}