using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Persistance;
using Ardalis.Specification;
using Domain.Entities.Audit;
using Domain.Entities.Base;
using Infrastructure.Core.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Infrastructure.Core.CosmosDbData.Repository
{
    public abstract class CosmosDbRepository<T> : IRepository<T>, IContainerContext<T>
        where T : BaseEntity
    {
        private readonly Container _container;

        /// <summary>
        ///     Audit container that will store audit log for all entities.
        /// </summary>
        private readonly Container _auditContainer;

        protected CosmosDbRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            if (cosmosDbContainerFactory == null)
            {
                throw new ArgumentNullException(nameof(cosmosDbContainerFactory));
            }

            _container = cosmosDbContainerFactory.GetContainer(ContainerName).Container;
            _auditContainer = cosmosDbContainerFactory.GetContainer(CosmosDbConstants.AuditContainer).Container;
        }

        /// <summary>
        ///     Gets the name of the CosmosDB container.
        /// </summary>
        public abstract string ContainerName { get; }

        /// <summary>
        ///     Generate id.
        /// </summary>
        /// <param name="entity">The entity for which the id will be generated.</param>
        /// <returns>The generated id.</returns>
        public abstract string GenerateId(T entity);

        /// <summary>
        ///     Resolve the partition key.
        /// </summary>
        /// <param name="entityId">The entity for which the partition key will be resolved.</param>
        /// <returns>The partition key.</returns>
        public abstract PartitionKey ResolvePartitionKey(string entityId);

        /// <summary>
        ///     Generate id for the audit record.
        ///     All entities will share the same audit container,
        ///     so we can define this method here with virtual default implementation.
        ///     Audit records for different entities will use different partition key values,
        ///     so we are not limited to the 20G per logical partition storage limit.
        /// </summary>
        /// <param name="entity">The entity for which the id will be generated.</param>
        /// <returns>The generated id.</returns>
        public virtual string GenerateAuditId(Audit entity) => $"{entity.EntityId}:{Guid.NewGuid()}";

        /// <summary>
        ///     Resolve the partition key for the audit record.
        ///     All entities will share the same audit container,
        ///     so we can define this method here with virtual default implementation.
        ///     Audit records for different entities will use different partition key values,
        ///     so we are not limited to the 20G per logical partition storage limit.
        /// </summary>
        /// <param name="entityId">The entity for which the partition key will be resolved.</param>
        /// <returns>The partition key.</returns>
        public virtual PartitionKey ResolveAuditPartitionKey(string entityId) => new PartitionKey($"{entityId.Split(':')[0]}");

        public async Task AddItemAsync(T item)
        {
            item.Id = string.IsNullOrEmpty(item.Id) ? GenerateId(item) : item.Id;
            await _container.CreateItemAsync<T>(item, ResolvePartitionKey(item.Id));
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

        // Search data using SQL query string
        // This shows how to use SQL string to read data from Cosmos DB for demonstration purpose.
        // For production, try to use safer alternatives like Parameterized Query and LINQ if possible.
        // Using string can expose SQL Injection vulnerability, e.g. select * from c where c.id=1 OR 1=1.
        // String can also be hard to work with due to special characters and spaces when advanced querying like search and pagination is required.
        public async Task<IEnumerable<T>> GetItemsAsync(string query)
        {
            FeedIterator<T> resultSetIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query));
            List<T> results = new List<T>();
            while (resultSetIterator.HasMoreResults)
            {
                FeedResponse<T> response = await resultSetIterator.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        /// <inheritdoc cref="IRepository{T}.GetItemsAsync(ISpecification{T})"/>
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

        /// <inheritdoc cref="IRepository{T}.GetItemsCountAsync(ISpecification{T})"/>
        public async Task<int> GetItemsCountAsync(ISpecification<T> specification)
        {
            IQueryable<T> queryable = ApplySpecification(specification);
            return await queryable.CountAsync();
        }

        public async Task UpdateItemAsync(string id, T item)
        {
            // Audit
            await Audit(item);

            // Update
            await _container.UpsertItemAsync<T>(item, ResolvePartitionKey(id));
        }

        /// <summary>
        ///     Evaluate specification and return IQueryable.
        /// </summary>
        /// <param name="specification">The specification to evaluate.</param>
        /// <returns>The resulting IQueryable.</returns>
        private IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            CosmosDbSpecificationEvaluator<T> evaluator = new CosmosDbSpecificationEvaluator<T>();
            return evaluator.GetQuery(_container.GetItemLinqQueryable<T>(), specification);
        }

        /// <summary>
        ///     Audit a item by adding it to the audit container.
        /// </summary>
        /// <param name="item">The item to add.</param>
        private async Task Audit(T item)
        {
            Audit auditItem = new Audit(
                item.GetType().Name,
                item.Id,
                Newtonsoft.Json.JsonConvert.SerializeObject(item));

            auditItem.Id = GenerateAuditId(auditItem);
            await _auditContainer.CreateItemAsync<Audit>(auditItem, ResolveAuditPartitionKey(auditItem.Id));
        }
    }
}