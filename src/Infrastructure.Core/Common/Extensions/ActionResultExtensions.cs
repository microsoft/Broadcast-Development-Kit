// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using Application.Exceptions.Models;
using Infrastructure.Core.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Infrastructure.Core.Common.Extensions
{
    public static class ActionResultExtensions
    {
        private static readonly IDictionary<Type, Func<ObjectResult, ExceptionDetails>> _handlers = new Dictionary<Type, Func<ObjectResult, ExceptionDetails>>
        {
            { typeof(ErrorDetails), HandleErrorDetails },
            { typeof(ValidationErrorDetails), HandleValidationErrorDetails },
        };

        public static ExceptionDetails GetExceptionDetails(this ObjectResult result)
        {
            ExceptionDetails exceptionDetails = null;

            var detailsType = result.DeclaredType;
            if (_handlers.Any(e => e.Key.IsAssignableFrom(detailsType)))
            {
                var exceptionHandler = _handlers.First(e => e.Key.IsAssignableFrom(detailsType));
                exceptionDetails = exceptionHandler.Value.Invoke(result);
            }

            return exceptionDetails;
        }

        private static ExceptionDetails HandleErrorDetails(ObjectResult result)
        {
            var problemDetails = JsonConvert.DeserializeObject<ErrorDetails>(result.Value.ToString());

            var exceptionDetails = new ExceptionDetails
            {
                Status = problemDetails.Status,
                Title = problemDetails.Title,
                Detail = problemDetails.Detail,
                StackTrace = problemDetails.StackTrace,
                Identifier = problemDetails.Identifier,
                InnerException = problemDetails.InnerException,
            };

            return exceptionDetails;
        }

        private static ExceptionDetails HandleValidationErrorDetails(ObjectResult result)
        {
            var problemDetails = JsonConvert.DeserializeObject<ValidationErrorDetails>(result.Value.ToString());

            var exceptionDetails = new ExceptionDetails
            {
                Status = problemDetails.Status,
                Title = problemDetails.Title,
                Detail = problemDetails.Detail,
                StackTrace = problemDetails.StackTrace,
                Identifier = problemDetails.Identifier,
                InnerException = problemDetails.InnerException,
            };

            return exceptionDetails;
        }
    }
}
