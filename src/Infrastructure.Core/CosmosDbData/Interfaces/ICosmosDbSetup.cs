// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Threading.Tasks;

namespace Infrastructure.Core.CosmosDbData.Interfaces
{
    public interface ICosmosDbSetup
    {
        Task SetupDatabaseAsync();
    }
}
