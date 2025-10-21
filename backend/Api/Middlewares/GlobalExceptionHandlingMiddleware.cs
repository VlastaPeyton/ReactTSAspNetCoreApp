using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Api.Middlewares
{   
    /* Ako u kodu nemam nigde try-catch (iako u Controller imam), stoga ako dodje do exception negde u kodu, on se propagira response putanjom 
     sve do GlobalExceptionHandlingMiddleware koji ga uhvati (pogledaj Exception propagation.txt). Posto imam try-catch u Controller, tamo ceda se 
     uhvati exception pre nego ovde, ali nema veze, treba uvek postaviti GlobalExceptionHandlingMiddleware.
    */

    // Pogledaj Middleware.txt 

    // Ovo moze i cesto je, ali se tesko testira, pa necu da koristim => U Program.cs: app.UseMiddleware<GlobalExceptionHandlingMiddlewareBezInterface>();
    public class GlobalExceptionHandlingMiddlewareBezInterface
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddlewareBezInterface> _logger;
        public GlobalExceptionHandlingMiddlewareBezInterface(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddlewareBezInterface> logger)
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
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }
        }
    }

    // Ovo koristim, jer se lako testira zbog interface => u Program.cs moram prvo DI registrujem, kao AddTransient, a onda da dodam middleware u pipeline regularno 
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                ProblemDetails problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Type = "Server Error",
                    Title = "Server Error",
                    Detail = "An internal server has ocurred"
                };

                string problemdDetailsJson = JsonConvert.SerializeObject(problemDetails);
                await context.Response.WriteAsync(problemdDetailsJson);

                context.Response.ContentType = "application/json";
            }
        }
    }
}
