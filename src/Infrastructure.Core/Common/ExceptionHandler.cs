// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using Application.Exceptions;
using Domain.Exceptions;
using Infrastructure.Core.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure.Core.Common
{
    public static class ExceptionHandler
    {
        private static readonly IDictionary<Type, Func<Exception, string, bool, ILogger, LogLevel, ObjectResult>> _exceptionHandlers = new Dictionary<Type, Func<Exception, string, bool, ILogger, LogLevel, ObjectResult>>
        {
            { typeof(ApiValidationException), HandleValidationException },
            { typeof(EntityNotFoundException), HandleNotFoundException },
            { typeof(NotSuccessfulRequestException), HandleNotSuccesfulRequestException },
            { typeof(ServiceException), HandleServiceException },
            { typeof(CustomBaseException), HandleCustomBaseException },
        };

        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger, bool isProduction, string appIdentifier = null)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var guid = Guid.NewGuid();
                    var identifier = string.IsNullOrEmpty(appIdentifier) ? guid.ToString() : $"{appIdentifier}:{guid}";
                    var errorResult = GetObjectResult(context.Features.Get<IExceptionHandlerFeature>()?.Error, identifier, isProduction, logger);

                    context.Response.StatusCode = errorResult.StatusCode ?? StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(
                        JsonConvert.SerializeObject(
                            errorResult,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                NullValueHandling = NullValueHandling.Ignore,
                            }));
                });
            });
        }

        private static ObjectResult GetObjectResult(Exception exception, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            if (exception == null)
            {
                logger.Log(logLevel, "ExceptionHandler triggered but error from IExceptionHandlerFeature couldn't be retrieved. Identifier: {identifier}", identifier);

                var details = new ErrorDetails()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "SERVER_ERRROR",
                    Detail = "Server error",
                    Identifier = identifier,
                };

                return new ObjectResult(details)
                {
                    DeclaredType = details.GetType(),
                };
            }

            ObjectResult result;
            Type type = exception.GetType();

            if (_exceptionHandlers.Any(e => e.Key.IsAssignableFrom(type)))
            {
                var exceptionHandler = _exceptionHandlers.First(e => e.Key.IsAssignableFrom(type));
                result = exceptionHandler.Value.Invoke(exception, identifier, isProductionEnvironment, logger, logLevel);
                return result;
            }

            result = HandleUnknownException(exception, identifier, isProductionEnvironment, logger, logLevel);

            return result;
        }

        private static ObjectResult HandleUnknownException(Exception ex, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            logger.Log(logLevel, "Exception found (UnknownException) - Type: {Type} - ErrorMessage: {Message}, Identifier: {identifier}, Trace: {StackTrace}", ex.GetType().ToString(), ex.Message, identifier, ex.StackTrace);

            ErrorDetails details = new ErrorDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = ex?.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Identifier = identifier,
            };

            if (!isProductionEnvironment)
            {
                details.InnerException = ex?.InnerException?.Message;
                details.StackTrace = ex?.StackTrace;
            }

            return new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                DeclaredType = !isProductionEnvironment ? details.GetType() : default,
            };
        }

        private static ObjectResult HandleNotSuccesfulRequestException(Exception ex, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            NotSuccessfulRequestException exception = ex as NotSuccessfulRequestException;

            logger.Log(logLevel, "Exception found (NotSuccessfulRequestException) - ErrorMessage: {Message}, Identifier: {identifier}, Trace: {StackTrace}", exception.Message, identifier, exception.StackTrace);

            ErrorDetails details = new ErrorDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = exception.RequestDetails.Title,
                Detail = exception.RequestDetails.Detail,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Identifier = identifier,
            };

            if (!isProductionEnvironment)
            {
                details.InnerException = exception.RequestDetails.InnerException;
                details.StackTrace = exception.RequestDetails.StackTrace;
            }

            return new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                DeclaredType = !isProductionEnvironment ? details.GetType() : default,
            };
        }

        private static ObjectResult HandleServiceException(Exception ex, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            ServiceException exception = ex as ServiceException;

            logger.Log(logLevel, "Exception found (ServiceException) - ErrorMessage: {Message}, Identifier: {identifier}, Trace: {StackTrace}", exception.Message, identifier, exception.StackTrace);

            ErrorDetails details = new ErrorDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error ocurred while trying to communicate with Graph API",
                Detail = exception.Error.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Identifier = identifier,
            };

            if (!isProductionEnvironment)
            {
                details.InnerException = exception.InnerException?.Message;
                details.StackTrace = exception.StackTrace;
            }

            return new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                DeclaredType = !isProductionEnvironment ? details.GetType() : default,
            };
        }

        private static ObjectResult HandleValidationException(Exception ex, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            ApiValidationException exception = ex as ApiValidationException;

            logger.Log(logLevel, "Exception found (ApiValidationException) - ErrorMessage: {Message}, Identifier: {identifier}, Trace: {StackTrace}", exception.Message, identifier, exception.StackTrace);

            ValidationErrorDetails details = new ValidationErrorDetails(exception.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Identifier = identifier,
            };

            if (!isProductionEnvironment)
            {
                details.InnerException = ex?.InnerException?.Message;
                details.StackTrace = ex?.StackTrace;
            }

            return new BadRequestObjectResult(details)
            {
                DeclaredType = !isProductionEnvironment ? details.GetType() : default,
            };
        }

        private static ObjectResult HandleNotFoundException(Exception ex, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            EntityNotFoundException exception = ex as EntityNotFoundException;

            logger.Log(logLevel, "Exception found (EntityNotFoundException) - ErrorMessage: {Message}, Identifier: {identifier}, Trace: {StackTrace}", exception.Message, identifier, exception.StackTrace);

            ErrorDetails details = new ErrorDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The specified resource was not found.",
                Detail = !isProductionEnvironment ? exception.Message : "SERVER_ERROR",
                Identifier = identifier,
            };

            if (!isProductionEnvironment)
            {
                details.InnerException = ex?.InnerException?.Message;
                details.StackTrace = ex?.StackTrace;
            }

            return new NotFoundObjectResult(details)
            {
                DeclaredType = details.GetType(),
            };
        }

        private static ObjectResult HandleCustomBaseException(Exception ex, string identifier, bool isProductionEnvironment, ILogger logger, LogLevel logLevel = LogLevel.Error)
        {
            logger.Log(logLevel, "Exception found (CustomeBaseException) - Type: {Type} - ErrorMessage: {Message}, Identifier: {identifier}, Trace: {StackTrace}", ex.GetType().ToString(), ex.Message, identifier, ex.StackTrace);

            CustomBaseException exception = ex as CustomBaseException;

            ErrorDetails details = new ErrorDetails
            {
                Status = (int)exception.StatusCode,
                Title = exception.Title,
                Detail = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Identifier = identifier,
            };

            if (!isProductionEnvironment)
            {
                details.InnerException = ex?.InnerException?.Message;
                details.StackTrace = ex?.StackTrace;
            }

            return new ObjectResult(details)
            {
                StatusCode = details.Status,
                DeclaredType = !isProductionEnvironment ? details.GetType() : default,
            };
        }
    }
}
