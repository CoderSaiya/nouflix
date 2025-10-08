﻿using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NouFlix.Models.Common;

namespace NouFlix.Configuration;

public class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger
    ) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            EmailAlreadyUsedException => (StatusCodes.Status409Conflict, "Email đã được sử dụng"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Không được phép"),
            NotFoundException => (StatusCodes.Status404NotFound, "Không tìm thấy"),
            DomainValidationException => (StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ"),
            ValidationException
                or System.ComponentModel.DataAnnotations.ValidationException
                => (StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ"),
            _ => (StatusCodes.Status500InternalServerError, "Lỗi không mong muốn")
        };

        if (status >= 500)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = GetDetail(exception),
            Instance = httpContext.Request.Path,
            Type = GetTypeUri(status) // gợi ý đường dẫn docs theo status
        };
        
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        if (exception is ValidationException fv)
        {
            var errors = fv.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
            problem.Extensions["errors"] = errors;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,  
            ProblemDetails = problem,
            Exception = exception
        });
    }
    
    private static string GetDetail(Exception ex) => ex.Message;

    private static string GetTypeUri(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "https://httpstatuses.io/400",
        StatusCodes.Status401Unauthorized => "https://httpstatuses.io/401",
        StatusCodes.Status404NotFound => "https://httpstatuses.io/404",
        StatusCodes.Status409Conflict => "https://httpstatuses.io/409",
        StatusCodes.Status429TooManyRequests => "https://httpstatuses.io/429",
        StatusCodes.Status500InternalServerError => "https://httpstatuses.io/500",
        _ => "about:blank"
    };
}