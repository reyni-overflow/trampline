using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Enums;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Microsoft.AspNetCore.SignalR;
using Trampline.Application.Services;
using Trampline.Infrastructure.Postgres.Data;
using Trampline.Infrastructure.Postgres.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Trampline.Web;
using Trampline.Web.Hubs;
using Trampline.Core.Models;
using Trampline.Web.Middlewares;
using Trampline.Web.Services;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 26_214_400;
});

var defaultOrigins = new[] { "http://trampline.localhost", "http://trampline.localhost:3000", "https://trampline.localhost" };
var devOrigins = new[] { "http://localhost:5173", "http://localhost:4173", "http://localhost:3000" };

var configuredOrigins = builder.Configuration["CORS_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? (builder.Environment.IsDevelopment() ? [.. defaultOrigins, .. devOrigins] : defaultOrigins);

var corsOrigins = configuredOrigins
    .Concat(configuredOrigins
        .Where(o => o.StartsWith("http://"))
        .Select(o => "https://" + o[7..]))
    .Distinct()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization", "Accept", "Accept-Language", "X-Requested-With", "X-SignalR-User-Agent")
            .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableBuiltInModelValidation = true;
    config.ValidationStrategy = ValidationStrategy.All;
    config.EnableBodyBindingSourceAutomaticValidation = true;
    config.EnableFormBindingSourceAutomaticValidation = true;
    config.EnableQueryBindingSourceAutomaticValidation = true;
    config.EnablePathBindingSourceAutomaticValidation = true;
    config.EnableCustomBindingSourceAutomaticValidation = true;
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        var signingKeys = new List<SecurityKey>
        {
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
        var previousKey = builder.Configuration["Jwt:PreviousKey"];
        if (!string.IsNullOrEmpty(previousKey) && previousKey.Length >= 32)
            signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(previousKey)));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKeys = signingKeys
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                    return Task.CompletedTask;
                }

                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                }
                else
                {
                    context.Token = context.Request.Cookies["jwt_token"];
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrEmpty(jti))
                {
                    var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                    if (await tokenService.IsJwtBlacklistedAsync(jti, context.HttpContext.RequestAborted))
                    {
                        context.Fail("Token has been revoked");
                    }
                }
            }
        };
    }
);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());
var dpKeysPath = OperatingSystem.IsWindows()
    ? Path.Combine(AppContext.BaseDirectory, "data-protection-keys")
    : "/app/data-protection-keys";
Directory.CreateDirectory(dpKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("write", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("upload", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("admin", opt =>
    {
        opt.PermitLimit = 120;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("files", opt =>
    {
        opt.PermitLimit = 120;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("health", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.ConfigureServices();
builder.Services.ConfigureRepositories();
builder.Services.ConfigureValidators();
builder.Services.ConfigureOptions(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
if (jwtKey.Length < 32)
    throw new InvalidOperationException("JWT key must be at least 32 characters long. Check the 'Jwt:Key' configuration value.");

TotpEncryption.Initialize(jwtKey);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 2
});

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseResponseCompression();
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CsrfHeaderMiddleware>();

var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "files");
Directory.CreateDirectory(filesPath);

if (app.Services.GetRequiredService<Trampline.Core.Storage.IStorageService>() is Trampline.Application.Services.IO.MinioStorageService minioStorage)
{
    try { await minioStorage.EnsureBucketAsync(); }
    catch (Exception ex) { app.Logger.LogWarning(ex, "MinIO unavailable at startup — bucket init skipped"); }
}

app.MapGet("/files/{**path}", async (HttpContext ctx, string path, Trampline.Core.Storage.IStorageService storage) =>
{
    if (string.IsNullOrWhiteSpace(path) || path.Contains("..") || path.Contains('\\')
        || path.Contains("//") || Path.IsPathRooted(path))
        return Results.BadRequest("Invalid file path");

    var normalized = Path.GetFullPath(Path.Combine("/files", path));
    if (!normalized.StartsWith("/files/", StringComparison.OrdinalIgnoreCase))
        return Results.Forbid();

    var fullPath = $"/files/{path}";

    if (path.Contains("resume", StringComparison.OrdinalIgnoreCase) || path.StartsWith("resumes/"))
    {
        if (!ctx.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        if (!ctx.User.IsInRole("Admin"))
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !path.Contains(userId, StringComparison.OrdinalIgnoreCase))
                return Results.Forbid();
        }
    }

    var stream = await storage.GetAsync(fullPath, ctx.RequestAborted);
    if (stream == null) return Results.NotFound();

    var ext = Path.GetExtension(path).ToLower();
    var contentType = ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".mp4" => "video/mp4",
        ".webm" => "video/webm",
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };

    return Results.File(stream, contentType);
}).RequireRateLimiting("files");

app.UseRateLimiter();

app.MapGet("/health", async (AppDbContext db, IDistributedCache cache) =>
{
    var checks = new Dictionary<string, string>();

    try
    {
        await db.Database.CanConnectAsync();
        checks["postgres"] = "healthy";
    }
    catch
    {
        checks["postgres"] = "unhealthy";
    }

    try
    {
        await cache.SetStringAsync("health_check", "ok", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
        });
        checks["redis"] = "healthy";
    }
    catch
    {
        checks["redis"] = "unhealthy";
    }

    var isHealthy = checks.Values.All(v => v == "healthy");
    return Results.Json(new
    {
        status = isHealthy ? "healthy" : "degraded",
        timestamp = DateTime.UtcNow,
        checks
    }, statusCode: isHealthy ? 200 : 503);
}).RequireRateLimiting("health");

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapControllers();
await app.TryMigrateAsync();
app.Run();
