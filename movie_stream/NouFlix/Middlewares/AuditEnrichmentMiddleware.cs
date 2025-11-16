using System.Security.Claims;
using Serilog;
using Serilog.Context;

namespace NouFlix.Middlewares;

public class AuditEnrichmentMiddleware(RequestDelegate next)
{
    private readonly Serilog.ILogger _logger = Log.ForContext<AuditEnrichmentMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy CorrelationId
        var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = context.TraceIdentifier;

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User?.Identity?.Name
                     ?? "anonymous";

        var username = context.User?.Identity?.Name ?? "anonymous";

        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // Đặt response header cho client biết correlation id
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("Username", username))
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("UserAgent", userAgent))
        {
            await next(context);
        }
    }
}