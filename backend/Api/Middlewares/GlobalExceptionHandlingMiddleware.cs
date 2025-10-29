using System;
using System.Net;
using Api.Exceptions;
using Api.Exceptions_i_Result_pattern.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middlewares
{
    /* 
     U pocetku, moj kod je radio ovako: Middleware- > Controller -> Service -> Repository.
     U Repository nisam explicitno bacio greske jer tu se podrazumevaju implicitne built-in + neam try-catch.
     U Service sam explicitno bacio greske + iz Repository implicitne su se propagirale u Service, ali neam try-catch. 
     U Controller imam try-catch, pa ce sve greske iz Repository/Service da se propagiraju ovde i tu da se uhvate i da se klijentu posalje i odgovor i greska. 
     Bolja solucija je GlobalExceptionHandlingMiddleware koji ce da hvata greske, pa nema vise potrebe za try-catch u Controller stoga 
    klijentu iz Controller saljem samo odgovor, a gresku iz GlobalExceptionHandlingMiddleware.
     
     Pogledaj Middleware.txt i Exception driven error handling.txt
     
     Ovo isto radi za svaki controller.
    */

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

    // Ovo koristim, jer se lako testira zbog interface => u Program.cs moram prvo DI registrujem zbog interface, kao AddTransient, a onda da dodam middleware u pipeline regularno 
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
                await next(context); // propusta request dalje do controllera, servisa ....
            }
            // Svaki custom exception je nasledio Exception i zato ce, kao i Exception, biti ovde uhvacen i kreiran odgovorajuci response 
            catch (Exception ex)
            {
                _logger.LogError(ex, "GlobalExceptionHandlerMiddleware uhvatio exception thrown from services or repository");

                // Ako neki middleware, registrovan ispod GlobalExceptionHandlingMiddleware, krene slati odgovor pre nego ovde uhvati se greska - pogledaj Middleware.txt
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("Response already started in other middleware, cannot modify response.");
                    throw;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {   
                    // Account:

                    // Register endpoint je za ove exceptions slao klijentu StatusCode 500 
                    UserCreatedException or RoleAssignmentException => StatusCodes.Status500InternalServerError,

                    // Login endpoint je za ove exceptions slao klijentu StatusCode 401 
                    //WrongPasswordException or WrongUsernameException => StatusCodes.Status401Unauthorized, - postalo Result pattern jer nije neocekivana greska systema, vec biznis logika

                    // ForgotPassword endpoint je za ovaj exception slao klijentu StatusCode 200
                    ForgotPasswordException => StatusCodes.Status200OK,

                    // ResetPassword endpoint je za ove exceptions slao klijentu StatusCode 200
                    ResetPasswordException => StatusCodes.Status200OK,

                    // RefreshToken endpoint je za ovaj exception slao klijentu StatusCode 401 
                    RefreshTokenException => StatusCodes.Status401Unauthorized,

                    // Comment:

                    // GetById endpoint 
                    CommentNotFoundException => StatusCodes.Status404NotFound,
                    // Delete endpoint 
                    NotYourCommentException => StatusCodes.Status401Unauthorized,

                    // Stock:

                    // Portfolio:

                    // Svaki endpoint, u bilo kom controlleru, je slao klijentu StatusCode 500 ako se desio implicitni error u service/repository 
                    _ => StatusCodes.Status500InternalServerError
                };

                // Pogledaj ProblemDetails.txt
                ProblemDetails problemDetails = new ProblemDetails 
                { 
                    Status = context.Response.StatusCode,
                    Title = ex switch
                    {   
                        // Account: 

                        UserCreatedException or RoleAssignmentException => "Implicit internal server error u service/repository",
                        // WrongPasswordException or WrongUsernameException => "Unauthorized", - postalo Result pattern jer nije neocekivana greska systema, vec biznis logika
                        ForgotPasswordException => "Saljem 200OK da zavaram trag i da si zamenio i da nisi password",
                        ResetPasswordException => "Saljem 200OK da zavaram trag i da si resetovao i da nisi password",
                        RefreshTokenException => "Unauthorized",

                        // Comment:
                        CommentNotFoundException => "Comment not found",
                        NotYourCommentException => "Ne mozes brisati tudji komentar",

                        // Stock:

                        // Portfolio: 

                        _ => "Implicit internal server error u service/repository"
                    },
                    Detail = ex.Message,
                    Instance = context.Request.Path // Koji endpoint je izazvao gresku
                };
                
                await context.Response.WriteAsJsonAsync(problemDetails); // response.HasStarted = true
            }
        }
    }
}
