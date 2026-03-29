using FluentAssertions;
using Trampline.Core.Models.Worker;

namespace Trampline.Tests.Domain;

public class WorkerInfoTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = WorkerInfo.Create("МФТИ", 3, new DateTime(2023, 9, 1), new DateTime(2027, 6, 30));

        result.IsSuccess.Should().BeTrue();
        result.Value!.University.Should().Be("МФТИ");
        result.Value.Course.Should().Be(3);
    }

    [Fact]
    public void Create_TrimsUniversity()
    {
        var result = WorkerInfo.Create("  ИТМО  ", 1, new DateTime(2025, 9, 1), new DateTime(2029, 6, 30));

        result.IsSuccess.Should().BeTrue();
        result.Value!.University.Should().Be("ИТМО");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUniversity_ReturnsFailure(string university)
    {
        var result = WorkerInfo.Create(university, 1, DateTime.UtcNow, DateTime.UtcNow.AddYears(4));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "university");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidCourse_ReturnsFailure(int course)
    {
        var result = WorkerInfo.Create("МФТИ", course, DateTime.UtcNow, DateTime.UtcNow.AddYears(4));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "course");
    }

    [Fact]
    public void Create_WithCourseGreaterThan6_ReturnsFailure()
    {
        var result = WorkerInfo.Create("МФТИ", 7, DateTime.UtcNow, DateTime.UtcNow.AddYears(4));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "course");
    }

    [Fact]
    public void Create_WithGraduationBeforeAdmission_ReturnsFailure()
    {
        var result = WorkerInfo.Create("МФТИ", 1, new DateTime(2027, 9, 1), new DateTime(2023, 6, 30));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "graduationAt");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    public void Create_WithValidCourses_Succeeds(int course)
    {
        var result = WorkerInfo.Create("ВУЗ", course, new DateTime(2023, 9, 1), new DateTime(2027, 6, 30));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Course.Should().Be(course);
    }
}
