namespace AuthenticationModule.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var h = context.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["X-XSS-Protection"] = "1; mode=block";
            if (context.Request.IsHttps) h["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            h["Referrer-Policy"] = "strict-origin-when-cross-origin";
            h["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            h["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; font-src 'self' data:; connect-src 'self';";
            await _next(context);
        }
    }
}