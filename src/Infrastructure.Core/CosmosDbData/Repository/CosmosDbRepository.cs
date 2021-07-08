// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Ardalis.Specification;
using Domain.Entities.Base;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public abstract class CosmosDbRepository<T> : IRepository<T>
        where T : CosmosDbEntity
    {
        private readonly Container _container;

        protected CosmosDbRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            if (cosmosDbContainerFactory == null)
            {
                throw new ArgumentNullException(nameof(cosmosDbContainerFactory));
            }

            _container = cosmosDbContainerFactory.GetContainer(ContainerName);
        }

        public abstract string ContainerName { get; }

        public abstract string GenerateId(T entity);

        public abstract PartitionKey ResolvePartitionKey(string entityId);

        public async Task AddItemAsync(T item)
        {
            item.Id = string.IsNullOrEmpty(item.Id) ? GenerateId(item) : item.Id;
            await _container.CreateItemAsync<T>(item, ResolvePartitionKey(item.Id));
        }

        public async Task UpdateItemAsync(string id, T item)
        {
            await _container.UpsertItemAsync<T>(item, ResolvePartitionKey(id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<T>(id, ResolvePartitionKey(id));
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<T> response = await _container.ReadItemAsync<T>(id, ResolvePartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<T> GetFirstItemAsync(ISpecification<T> specification)
        {
            IQueryable<T> queryable = ApplySpecification(specification);
            FeedIterator<T> iterator = queryable.ToFeedIterator<T>();

            if (iterator.HasMoreResults)
            {
                FeedResponse<T> response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }

        public async Task<IEnumerable<T>> GetItemsAsync(ISpecification<T> specification)
        {
            IQueryable<T> queryable = ApplySpecification(specification);
            FeedIterator<T> iterator = queryable.ToFeedIterator<T>();

            List<T> results = new List<T>();
            while (iterator.HasMoreResults)
            {
                FeedResponse<T> response = await iterator.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<int> GetItemsCountAsync(ISpecification<T> specification)
        {
            IQueryable<T> queryable = ApplySpecification(specification);
            return await queryable.CountAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            CosmosDbSpecificationEvaluator<T> evaluator = new CosmosDbSpecificationEvaluator<T>();
            return evaluator.GetQuery(_container.GetItemLinqQueryable<T>(), specification);
        }
    }
}
