using FluentAssertions;
using Trampline.Core.Models;

namespace Trampline.Tests.Domain;

public class FavoriteTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var favorite = Favorite.Create(userId, targetId, FavoriteType.Job);

        favorite.Id.Should().NotBeEmpty();
        favorite.UserId.Should().Be(userId);
        favorite.TargetId.Should().Be(targetId);
        favorite.Type.Should().Be(FavoriteType.Job);
        favorite.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(FavoriteType.Job)]
    [InlineData(FavoriteType.Company)]
    [InlineData(FavoriteType.Event)]
    public void Create_WithAllTypes_SetsCorrectType(FavoriteType type)
    {
        var favorite = Favorite.Create(Guid.NewGuid(), Guid.NewGuid(), type);

        favorite.Type.Should().Be(type);
    }
}
