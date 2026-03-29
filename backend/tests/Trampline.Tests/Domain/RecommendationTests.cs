using FluentAssertions;
using Trampline.Core.Models.Worker;

namespace Trampline.Tests.Domain;

public class RecommendationTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();
        var jobId = Guid.NewGuid();

        var rec = Recommendation.Create(fromUserId, toUserId, jobId, "Отличный кандидат!");

        rec.Id.Should().NotBeEmpty();
        rec.FromUserId.Should().Be(fromUserId);
        rec.ToUserId.Should().Be(toUserId);
        rec.JobId.Should().Be(jobId);
        rec.Message.Should().Be("Отличный кандидат!");
        rec.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNullMessage_AllowsNull()
    {
        var rec = Recommendation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        rec.Message.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsMessage()
    {
        var rec = Recommendation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  Рекомендую  ");

        rec.Message.Should().Be("Рекомендую");
    }
}
