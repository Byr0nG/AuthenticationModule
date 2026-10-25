using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthenticationModule.Filters
{
    public class AntiXssFilter : IAsyncPageFilter
    {
        private static readonly string[] Patterns =
            { @"<script.*?>.*?</script>", @"javascript:", @"onerror=", @"onload=", @"onclick=", @"eval\(", @"expression\(" };

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext ctx, PageHandlerExecutionDelegate next)
        {
            foreach (var p in ctx.HandlerInstance.GetType().GetProperties())
            {
                if (p.PropertyType != typeof(string) || !p.CanWrite || p.Name.Contains("Password")) continue;
                if (!p.GetCustomAttributes(typeof(BindPropertyAttribute), true).Any()) continue;
                if (p.GetValue(ctx.HandlerInstance) is string v && v.Length > 0)
                    p.SetValue(ctx.HandlerInstance, Sanitize(v));
            }
            await next();
        }

        private static string Sanitize(string input)
        {
            foreach (var pat in Patterns)
                input = Regex.Replace(input, pat, "", RegexOptions.IgnoreCase);
            return input.Trim();
        }
    }
}