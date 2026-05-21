using System.Net;
using System.Text.Json;

namespace TurnosMedicos.Middlewares
{
    public class ExceptionMiddlewere
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddlewere> _logger;

        public ExceptionMiddlewere(RequestDelegate next, ILogger<ExceptionMiddlewere> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ocurrio una excepcion no controlada: {ex.Message}");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new ErrorResponse
                {
                    StatusCode = context.Response.StatusCode,
                    Mensaje = $"Ocurrio un error interno del servidor: {ex.Message}"
                };

                var json = JsonSerializer.Serialize(response);

                await context.Response.WriteAsync(json);
            }
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
