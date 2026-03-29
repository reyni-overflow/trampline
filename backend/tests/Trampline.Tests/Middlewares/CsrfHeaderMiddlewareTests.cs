using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using Trampline.Web.Middlewares;

namespace Trampline.Tests.Middlewares;

public class CsrfHeaderMiddlewareTests
{
    private static readonly RequestDelegate OkDelegate = context =>
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        return Task.CompletedTask;
    };

    private static CsrfHeaderMiddleware CreateSut()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Development");
        return new CsrfHeaderMiddleware(OkDelegate, config, env.Object);
    }

    private readonly CsrfHeaderMiddleware _sut = CreateSut();

    [Fact]
    public async Task Post_WithoutXRequestedWithHeader_Returns403()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/test";

        await _sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Post_WithCorrectXRequestedWithHeader_Returns200()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        await _sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Post_WithWrongXRequestedWithValue_Returns403()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Requested-With"] = "wrong-value";

        await _sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Post_WithLowercaseXRequestedWithValue_Returns200()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Requested-With"] = "xmlhttprequest";

        await _sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_WithoutXRequestedWithHeader_Returns200()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/test";

        await _sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Post_ToExcludedHubsPath_WithoutHeader_Returns200()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/hubs/notifications";

        await _sut.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
