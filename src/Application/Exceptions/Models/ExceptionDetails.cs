// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Exceptions.Models
{
    public class ExceptionDetails
    {
        public int? Status { get; set; }

        public string Title { get;  set; }

        public string InnerException { get;  set; }

        public string Detail { get;  set; }

        public string StackTrace { get;  set; }

        public string Identifier { get;  set; }
    }
}
