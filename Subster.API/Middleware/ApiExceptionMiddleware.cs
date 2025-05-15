using Subster.Models;
using Subster.API.Exceptions;

namespace Subster.API.Middleware;

// This middleware handles exceptions thrown during the request pipeline
public class ApiExceptionMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

	public async Task InvokeAsync(HttpContext ctx)
  {
    try
    {
      await _next(ctx);
    }
    catch (Exception ex)
    {
        // Get the status code based on the exception type
        var status = ex switch
        {
            NotFoundException _    => 404,
            ValidationException _  => 400,
            ArgumentException _    => 400,
            UnauthorizedException _ => 401,
            InvalidOperationException _=> 409,
            _                      => 500
        };

        // Map the message to the ApiError object, and don't expose the exception message to the client if it's a server error
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
