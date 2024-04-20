using Serilog;

namespace App.Middlewares
{
    public static class LogEnricher
    {
        /// <summary>
        /// Enriches the HTTP request log with additional data via the Diagnostic Context
        /// </summary>
        /// <param name="diagnosticContext">The Serilog diagnostic context</param>
        /// <param name="httpContext">The current HTTP Context</param>
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme.ToUpper());
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("Query", httpContext.Request.QueryString.Value);
            //diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        }
    }
}
