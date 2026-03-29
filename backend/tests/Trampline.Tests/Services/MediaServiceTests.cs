using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services.IO;

namespace Trampline.Tests.Services;

public class MediaServiceTests
{
    private readonly Mock<ILogger<MediaService>> _loggerMock = new();
    private readonly Mock<Trampline.Core.Storage.IStorageService> _storageMock = new();
    private readonly MediaService _sut;

    public MediaServiceTests()
    {
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream _, string path, string _, CancellationToken _) => path);
        _sut = new MediaService(_loggerMock.Object, _storageMock.Object);
    }

    private static Mock<IFormFile> CreateMockFile(string contentType, long length, byte[] header)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.FileName).Returns("test-file");

        var ms = new MemoryStream(header.Length > 0 ? header : new byte[1]);
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        return fileMock;
    }

    #region File Size Validation

    [Fact]
    public async Task UploadFile_ZeroLength_ReturnsFailure()
    {
        var file = CreateMockFile("image/jpeg", 0, new byte[] { 0xFF, 0xD8, 0xFF });

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("size"));
    }

    [Fact]
    public async Task UploadFile_NegativeLength_ReturnsFailure()
    {
        var file = CreateMockFile("image/jpeg", -1, new byte[] { 0xFF, 0xD8, 0xFF });

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_ExceedsMaxSize_ReturnsFailure()
    {
        var file = CreateMockFile("image/jpeg", 100_000_001, new byte[] { 0xFF, 0xD8, 0xFF });

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("100 MB"));
    }

    [Fact]
    public async Task UploadFile_ExactMaxSize_IsAccepted()
    {
        var jpgHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var file = CreateMockFile("image/jpeg", 100_000_000, jpgHeader);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Content Type Validation

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/json")]
    [InlineData("text/html")]
    [InlineData("application/xml")]
    [InlineData("application/javascript")]
    public async Task UploadFile_UnsupportedContentType_ReturnsFailure(string contentType)
    {
        var file = CreateMockFile(contentType, 100, new byte[] { 0x00 });

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not allowed"));
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("video/mp4")]
    [InlineData("video/webm")]
    [InlineData("application/pdf")]
    [InlineData("application/msword")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public async Task UploadFile_SupportedContentTypes_AreRecognized(string contentType)
    {
        byte[] header = contentType switch
        {
            "image/jpeg" => new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 },
            "image/png" => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 },
            "image/webp" => new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 },
            "video/mp4" => new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 },
            "video/webm" => new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x00, 0x00, 0x00, 0x00 },
            "application/pdf" => new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x00, 0x00, 0x00 },
            "application/msword" => new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0x00, 0x00, 0x00, 0x00 },
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00 },
            _ => new byte[8],
        };

        var file = CreateMockFile(contentType, 1024, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Magic Bytes Validation

    [Fact]
    public async Task UploadFile_JpegWithWrongMagicBytes_ReturnsFailure()
    {
        var wrongHeader = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("image/jpeg", 1024, wrongHeader);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("does not match"));
    }

    [Fact]
    public async Task UploadFile_PngWithJpegMagicBytes_ReturnsFailure()
    {
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("image/png", 1024, jpegHeader);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("does not match"));
    }

    [Fact]
    public async Task UploadFile_PdfWithWrongMagicBytes_ReturnsFailure()
    {
        var wrongHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("application/pdf", 1024, wrongHeader);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_TooFewBytesRead_ReturnsFailure()
    {
        var shortHeader = new byte[] { 0xFF, 0xD8, 0xFF };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.Length).Returns(3);
        fileMock.Setup(f => f.FileName).Returns("tiny.jpg");

        var ms = new MemoryStream(shortHeader);
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var result = await _sut.UploadFile(fileMock.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("does not match"));
    }

    [Fact]
    public async Task UploadFile_Mp4WithVariant1MagicBytes_Succeeds()
    {
        var header = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 };
        var file = CreateMockFile("video/mp4", 5000, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_Mp4WithVariant2MagicBytes_Succeeds()
    {
        var header = new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 };
        var file = CreateMockFile("video/mp4", 5000, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_Mp4WithVariant3MagicBytes_Succeeds()
    {
        var header = new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 };
        var file = CreateMockFile("video/mp4", 5000, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Subdirectory Routing

    [Fact]
    public async Task UploadFile_JpegImage_SavesToPhotos()
    {
        var header = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("image/jpeg", 1024, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/photos/");
        result.Value.Should().EndWith(".jpg");
    }

    [Fact]
    public async Task UploadFile_PngImage_SavesToPhotos()
    {
        var header = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("image/png", 1024, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/photos/");
        result.Value.Should().EndWith(".png");
    }

    [Fact]
    public async Task UploadFile_Mp4Video_SavesToVideos()
    {
        var header = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 };
        var file = CreateMockFile("video/mp4", 5000, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/videos/");
        result.Value.Should().EndWith(".mp4");
    }

    [Fact]
    public async Task UploadFile_WebmVideo_SavesToVideos()
    {
        var header = new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("video/webm", 5000, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/videos/");
        result.Value.Should().EndWith(".webm");
    }

    [Fact]
    public async Task UploadFile_Pdf_SavesToResumes()
    {
        var header = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("application/pdf", 2048, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/resumes/");
        result.Value.Should().EndWith(".pdf");
    }

    [Fact]
    public async Task UploadFile_Docx_SavesToResumes()
    {
        var header = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("application/vnd.openxmlformats-officedocument.wordprocessingml.document", 2048, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/resumes/");
        result.Value.Should().EndWith(".docx");
    }

    [Fact]
    public async Task UploadFile_Doc_SavesToResumes()
    {
        var header = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("application/msword", 2048, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/files/resumes/");
        result.Value.Should().EndWith(".doc");
    }

    #endregion

    #region Unique File Names

    [Fact]
    public async Task UploadFile_GeneratesUniqueFileNames()
    {
        var header = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 };

        var file1 = CreateMockFile("image/jpeg", 1024, header);
        var file2 = CreateMockFile("image/jpeg", 1024, header);

        var result1 = await _sut.UploadFile(file1.Object, CancellationToken.None);
        var result2 = await _sut.UploadFile(file2.Object, CancellationToken.None);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().NotBe(result2.Value);
    }

    #endregion

    #region Case Insensitive Content Type

    [Fact]
    public async Task UploadFile_UppercaseContentType_IsHandled()
    {
        var header = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 };
        var file = CreateMockFile("Image/JPEG", 1024, header);

        var result = await _sut.UploadFile(file.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
