namespace Trampline.Web.Middlewares;

public class CsrfHeaderMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment env)
{
    private static readonly HashSet<string> MutatingMethods = ["POST", "PUT", "DELETE", "PATCH"];

    private readonly HashSet<string> _allowedOrigins = BuildAllowedOrigins(configuration, env);

    public async Task InvokeAsync(HttpContext context)
    {
        if (MutatingMethods.Contains(context.Request.Method) && !IsExcludedPath(context.Request.Path))
        {
            if (!context.Request.Headers.TryGetValue("X-Requested-With", out var xrw) ||
                !string.Equals(xrw, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                await WriteForbidden(context, "Missing required security header");
                return;
            }

            var origin = context.Request.Headers.Origin.FirstOrDefault();
            if (!string.IsNullOrEmpty(origin) && !_allowedOrigins.Contains(origin))
            {
                await WriteForbidden(context, "Origin not allowed");
                return;
            }
        }

        await next(context);
    }

    private static async Task WriteForbidden(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { title = "Forbidden", status = 403, detail });
    }

    private static bool IsExcludedPath(PathString path) =>
        path.StartsWithSegments("/hubs") ||
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/swagger") ||
        path.StartsWithSegments("/scalar") ||
        path.StartsWithSegments("/openapi");

    private static HashSet<string> BuildAllowedOrigins(IConfiguration configuration, IHostEnvironment env)
    {
        var defaults = new[] { "http://trampline.localhost", "http://trampline.localhost:3000", "https://trampline.localhost" };
        var dev = new[] { "http://localhost:5173", "http://localhost:4173", "http://localhost:3000" };

        var configured = configuration["CORS_ORIGINS"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var origins = configured ?? (env.IsDevelopment() ? [.. defaults, .. dev] : defaults);

        var set = new HashSet<string>(origins, StringComparer.OrdinalIgnoreCase);
        foreach (var o in origins.Where(o => o.StartsWith("http://")))
            set.Add("https://" + o[7..]);

        return set;
    }
}
