// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.Exceptions
{
    [Serializable]
    public class ApiValidationException : Exception
    {
        public ApiValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
            ModelState = new ModelStateDictionary();
        }

        public ApiValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

            foreach (var failure in failures)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
        }

        protected ApiValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public IDictionary<string, string[]> Errors { get; }

        public ModelStateDictionary ModelState { get; }
    }
}
