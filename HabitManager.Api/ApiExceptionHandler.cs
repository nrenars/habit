using HabitManager.Application;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HabitManager.Api;

public class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Nav atrasts"),
            BadHttpRequestException e => (e.StatusCode, "Nederīgs pieprasījums"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Nederīgi dati"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Darbība nav atļauta"),
            _ => (0, "")
        };

        if (status == 0) return false;   

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = exception.Message }
        });
    }
}
