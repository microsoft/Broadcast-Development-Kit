// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Domain.Entities.Base;
using Domain.Entities.Parts;
using Domain.Enums;

namespace Domain.Entities
{
    public class Service : CosmosDbEntity
    {
        public string CallId { get; set; }

        public string Name { get; set; }

        public ServiceState State { get; set; }

        public DateTime CreatedAt { get; set; }

        public Infrastructure Infrastructure { get; set; }
    }
}
