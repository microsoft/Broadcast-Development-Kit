// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Common.Config
{
    public class CosmosDbConfiguration
    {
        /// <summary>
        ///     Gets or sets the CosmosDb Account - The Azure Cosmos DB endpoint.
        /// </summary>
        public string EndpointUrl { get; set; }

        /// <summary>
        ///     Gets or sets the Key - The primary key for the Azure DocumentDB account.
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        ///    Gets or sets the  Database nam.
        /// </summary>
        public string DatabaseName { get; set; }
    }
}