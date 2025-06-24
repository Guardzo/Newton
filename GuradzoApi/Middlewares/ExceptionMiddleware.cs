using GuradzoApi.Models;
using GuradzoApi.Services;
using System.Net;
using System.Text.Json;

namespace GuradzoApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _customLogger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILoggerService customLogger, IHostEnvironment env)
        {
            _next = next;
            _customLogger = customLogger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _customLogger.LogError("Unhandled exception occurred", ex);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                        ? new ApiError
                            {
                                Status = context.Response.StatusCode,
                                Message = ex.Message,
                                StackTrace = ex.StackTrace
                            }
                        : new ApiError
                            {
                                Status = context.Response.StatusCode,
                                Message = "An internal server error occurred."
                            };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
