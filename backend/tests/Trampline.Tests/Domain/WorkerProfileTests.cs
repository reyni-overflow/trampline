using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Worker;

namespace Trampline.Tests.Domain;

public class WorkerProfileTests
{
    private static WorkerProfile CreateTestProfile() =>
        WorkerProfile.Create(Guid.NewGuid(), "Иван", "Иванов", "Иванович", null, "О себе", null);

    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var info = new WorkerInfo("МФТИ", 3, new DateTime(2023, 9, 1), new DateTime(2027, 6, 30));

        var profile = WorkerProfile.Create(userId, "Алексей", "Петров", "Сергеевич", info, "Люблю код", "/photos/avatar.jpg");

        profile.Id.Should().NotBeEmpty();
        profile.UserId.Should().Be(userId);
        profile.Name.Should().Be("Алексей");
        profile.LastName.Should().Be("Петров");
        profile.Patronymic.Should().Be("Сергеевич");
        profile.Info.Should().Be(info);
        profile.About.Should().Be("Люблю код");
        profile.Photo.Should().Be("/photos/avatar.jpg");
        profile.Resume.Should().BeNull();
        profile.Skills.Should().BeEmpty();
        profile.Repos.Should().BeEmpty();
        profile.JobApplications.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullOptionalFields_AllowsNulls()
    {
        var profile = WorkerProfile.Create(Guid.NewGuid(), "Name", "Last", "Pat", null, null, null);

        profile.Info.Should().BeNull();
        profile.About.Should().BeNull();
        profile.Photo.Should().BeNull();
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var profile = CreateTestProfile();

        profile.Update("Новое", "Фамилия", "Отчество", "Новое о себе", "/new-photo.jpg");

        profile.Name.Should().Be("Новое");
        profile.LastName.Should().Be("Фамилия");
        profile.Patronymic.Should().Be("Отчество");
        profile.About.Should().Be("Новое о себе");
        profile.Photo.Should().Be("/new-photo.jpg");
    }

    [Fact]
    public void Update_TrimsValues()
    {
        var profile = CreateTestProfile();

        profile.Update("  Имя  ", "  Фам  ", "  Отч  ", "  Описание  ", "  /photo.jpg  ");

        profile.Name.Should().Be("Имя");
        profile.LastName.Should().Be("Фам");
        profile.Patronymic.Should().Be("Отч");
        profile.About.Should().Be("Описание");
        profile.Photo.Should().Be("/photo.jpg");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ThrowsDomainException(string name)
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update(name, "Last", "Pat", null, null);

        act.Should().Throw<DomainException>().WithMessage("*Name*required*");
    }

    [Fact]
    public void Update_WithShortName_ThrowsDomainException()
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update("A", "Last", "Pat", null, null);

        act.Should().Throw<DomainException>().WithMessage("*2 characters*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyLastName_ThrowsDomainException(string lastName)
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update("Name", lastName, "Pat", null, null);

        act.Should().Throw<DomainException>().WithMessage("*LastName*required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyPatronymic_ThrowsDomainException(string patronymic)
    {
        var profile = CreateTestProfile();

        var act = () => profile.Update("Name", "Last", patronymic, null, null);

        act.Should().Throw<DomainException>().WithMessage("*Patronymic*required*");
    }

    [Fact]
    public void SetResume_WithValidPath_SetsResume()
    {
        var profile = CreateTestProfile();

        profile.SetResume("/files/resumes/resume.pdf");

        profile.Resume.Should().Be("/files/resumes/resume.pdf");
    }

    [Fact]
    public void SetResume_TrimsPath()
    {
        var profile = CreateTestProfile();

        profile.SetResume("  /files/resumes/resume.pdf  ");

        profile.Resume.Should().Be("/files/resumes/resume.pdf");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetResume_WithEmptyValue_ThrowsDomainException(string resume)
    {
        var profile = CreateTestProfile();

        var act = () => profile.SetResume(resume);

        act.Should().Throw<DomainException>().WithMessage("*Resume*required*");
    }

    [Fact]
    public void UpdateSkills_ReplacesList()
    {
        var profile = CreateTestProfile();
        var skills = new List<string> { "C#", "Python", "SQL" };

        profile.UpdateSkills(skills);

        profile.Skills.Should().HaveCount(3);
        profile.Skills.Should().Contain("C#");
    }

    [Fact]
    public void UpdateRepos_ReplacesList()
    {
        var profile = CreateTestProfile();
        var repos = new List<string> { "https://github.com/user/repo1", "https://github.com/user/repo2" };

        profile.UpdateRepos(repos);

        profile.Repos.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateInfo_SetsNewInfo()
    {
        var profile = CreateTestProfile();
        var info = new WorkerInfo("МГУ", 2, new DateTime(2024, 9, 1), new DateTime(2028, 6, 30));

        profile.UpdateInfo(info);

        profile.Info.Should().Be(info);
        profile.Info!.University.Should().Be("МГУ");
        profile.Info.Course.Should().Be(2);
    }
}
