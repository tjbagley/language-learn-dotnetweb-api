using LanguageLearnNETWebAPI.Constants;

namespace LanguageLearnNETWebAPI.Middleware
{
    public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId =
                context.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var headerValue)
                && !string.IsNullOrWhiteSpace(headerValue)
                    ? headerValue.ToString()
                    : Guid.NewGuid().ToString();

            context.Items[CorrelationIdConstants.LogPropertyName] = correlationId;
            context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;

            using (logger.BeginScope(new { CorrelationId = correlationId }))
            {
                await next(context);
            }
        }
    }
}
