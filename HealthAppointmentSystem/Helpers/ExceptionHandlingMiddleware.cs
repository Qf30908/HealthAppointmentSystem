using Microsoft.ApplicationInsights;
using System.Net;
using System.Text.Json;

namespace HealthAppointmentSystem.Helpers
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate requestDelegate, TelemetryClient telemetry, ILogger<ExceptionHandlingMiddleware> logger) {

            _requestDelegate = requestDelegate;
            _telemetry = telemetry;
            _logger = logger;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _requestDelegate(httpContext);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                _logger.LogError(ex, "Unexcpected Error occured");

                httpContext.Response.ContentType="application/json";
                httpContext.Response.StatusCode=(int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    Message = "As error occured while processing your request ",
                    Details = ex.Message
                };

                var json = JsonSerializer.Serialize(response);
                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
