using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Core.Common.Models
{
    public class ErrorDetails : ProblemDetails
    {
        public ErrorDetails()
        {
        }

        public string StackTrace { get; set; }

        public string InnerException { get; set; }

        public string Identifier { get; internal set; }
    }
}
