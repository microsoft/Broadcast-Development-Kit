// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Newtonsoft.Json;

namespace Domain.Entities.Base
{
    public abstract class CosmosDbEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
