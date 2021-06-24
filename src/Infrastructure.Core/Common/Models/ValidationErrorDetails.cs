using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Infrastructure.Core.Common.Models
{
    public class ValidationErrorDetails: ValidationProblemDetails
    {
        public ValidationErrorDetails(ModelStateDictionary modelState) : base(modelState) { }
        public string StackTrace { get; set; }

        public string InnerException { get; set; }

        public string Identifier { get; internal set; }
    }
}
