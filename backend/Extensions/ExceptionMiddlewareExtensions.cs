using TurnosMedicos.Middlewares;

namespace TurnosMedicos.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(
        this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddlewere>();
        }
    }
}
