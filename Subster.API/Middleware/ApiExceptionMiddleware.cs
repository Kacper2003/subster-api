using Subster.Models;
using Subster.API.Exceptions;

namespace Subster.API.Middleware;

public class ApiExceptionMiddleware
{
  private readonly RequestDelegate _next;
  public ApiExceptionMiddleware(RequestDelegate next) => _next = next;

  public async Task InvokeAsync(HttpContext ctx)
  {
    try
    {
      await _next(ctx);
    }
    catch (Exception ex)
    {
        var status = ex switch
        {
            NotFoundException _    => 404,
            ValidationException _  => 400,
            ArgumentException _    => 400,
            UnauthorizedException _ => 401,
            InvalidOperationException _=> 409,
            _                      => 500
        };

        var error = new ApiError {
            StatusCode    = status,
            Message = (status == 500) 
                ? "An unexpected error occurred." 
                : ex.Message
        };
        ctx.Response.StatusCode  = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(error);
    }
  }
}
