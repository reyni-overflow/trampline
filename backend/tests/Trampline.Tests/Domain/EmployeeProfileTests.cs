using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Employee;

namespace Trampline.Tests.Domain;

public class EmployeeProfileTests
{
    private static EmployeeProfile CreateTestProfile()
    {
        var info = new EmployeeInfo { INN = "1234567890", Email = "company@test.com", Address = "Москва" };
        return EmployeeProfile.Create(Guid.NewGuid(), "КодМастер", "IT-компания", "Разработка ПО", info, "https://kodmaster.ru");
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var info = new EmployeeInfo { INN = "9876543210", Email = "hr@company.com" };

        var profile = EmployeeProfile.Create(userId, "TestCompany", "Description", "IT", info, "https://site.com");

        profile.Id.Should().NotBeEmpty();
        profile.UserId.Should().Be(userId);
        profile.Name.Should().Be("TestCompany");
        profile.Description.Should().Be("Description");
        profile.Activity.Should().Be("IT");
        profile.Link.Should().Be("https://site.com");
        profile.Info.Should().Be(info);
        profile.IsVerified.Should().BeFalse();
        profile.Socials.Should().BeEmpty();
        profile.Photos.Should().BeEmpty();
        profile.Videos.Should().BeEmpty();
        profile.Jobs.Should().BeEmpty();
        profile.Events.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullLink_AllowsNull()
    {
        var info = new EmployeeInfo();
        var profile = EmployeeProfile.Create(Guid.NewGuid(), "Company", "Desc", "Activity", info, null);

        profile.Link.Should().BeNull();
    }

    [Fact]
    public void Update_WithValidData_UpdatesAllFields()
    {
        var profile = CreateTestProfile();

        profile.Update("NewName", "NewDescription", "NewActivity", "https://new-site.com");

        profile.Name.Should().Be("NewName");
        profile.Description.Should().Be("NewDescription");
        profile.Activity.Should().Be("NewActivity");
        profile.Link.Should().Be("https://new-site.com");
    }

    [Fact]
    public void Update_TrimsWhitespace()
    {
        var profile = CreateTestProfile();

        profile.Update("  Name  ", "  Desc  ", "  Activity  ", "  https://site.com  ");

        profile.Name.Should().Be("Name");
        profile.Description.Should().Be("Desc");
        profile.Activity.Should().Be("Activity");
        profile.Link.Should().Be("https://site.com");
    }

    [Theory]
    [InlineData("", "desc", "activity")]
    [InlineData("   ", "desc", "activity")]
    public void Update_WithEmptyName_ThrowsDomainException(string name, string desc, string activity)
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update(name, desc, activity, null);

        act.Should().Throw<DomainException>().WithMessage("*Name*required*");
    }

    [Fact]
    public void Update_WithShortName_ThrowsDomainException()
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update("A", "desc", "activity", null);

        act.Should().Throw<DomainException>().WithMessage("*2 characters*");
    }

    [Theory]
    [InlineData("Name", "", "activity")]
    [InlineData("Name", "   ", "activity")]
    public void Update_WithEmptyDescription_ThrowsDomainException(string name, string desc, string activity)
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update(name, desc, activity, null);

        act.Should().Throw<DomainException>().WithMessage("*Description*required*");
    }

    [Theory]
    [InlineData("Name", "desc", "")]
    [InlineData("Name", "desc", "   ")]
    public void Update_WithEmptyActivity_ThrowsDomainException(string name, string desc, string activity)
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update(name, desc, activity, null);

        act.Should().Throw<DomainException>().WithMessage("*Activity*required*");
    }

    [Fact]
    public void UpdateSocials_ReplacesSocialsList()
    {
        var profile = CreateTestProfile();
        var socials = new List<string> { "https://vk.com/company", "https://t.me/company" };

        profile.UpdateSocials(socials);

        profile.Socials.Should().HaveCount(2);
        profile.Socials.Should().Contain("https://vk.com/company");
    }

    [Fact]
    public void UpdateSocials_WithNull_SetsEmptyList()
    {
        var profile = CreateTestProfile();
        profile.UpdateSocials(new List<string> { "old" });

        profile.UpdateSocials(null!);

        profile.Socials.Should().BeEmpty();
    }

    [Fact]
    public void AddPhoto_AddsToPhotosList()
    {
        var profile = CreateTestProfile();

        profile.AddPhoto("/files/photos/office1.jpg");
        profile.AddPhoto("/files/photos/office2.jpg");

        profile.Photos.Should().HaveCount(2);
    }

    [Fact]
    public void AddVideo_AddsToVideosList()
    {
        var profile = CreateTestProfile();

        profile.AddVideo("/files/videos/promo.mp4");

        profile.Videos.Should().HaveCount(1);
        profile.Videos.Should().Contain("/files/videos/promo.mp4");
    }

    [Fact]
    public void Verify_SetsVerifiedTrue()
    {
        var profile = CreateTestProfile();

        profile.Verify();

        profile.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void Unverify_SetsVerifiedFalse()
    {
        var profile = CreateTestProfile();
        profile.Verify();

        profile.Unverify();

        profile.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void SetVerified_SetsExactValue()
    {
        var profile = CreateTestProfile();

        profile.SetVerified(true);
        profile.IsVerified.Should().BeTrue();

        profile.SetVerified(false);
        profile.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void UpdateInfo_ReplacesInfo()
    {
        var profile = CreateTestProfile();
        var newInfo = new EmployeeInfo { INN = "0000000000", Email = "new@company.com", Address = "СПб" };

        profile.UpdateInfo(newInfo);

        profile.Info.Should().Be(newInfo);
        profile.Info.INN.Should().Be("0000000000");
    }
}
