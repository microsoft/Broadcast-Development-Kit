// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;

namespace Application.Common.Models
{
    public class PagedQueryResult<T>
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public long TotalItems { get; set; }

        public IList<T> Items { get; set; }
    }
}