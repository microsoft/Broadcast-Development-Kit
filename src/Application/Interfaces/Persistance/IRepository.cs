// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.Specification;
using Domain.Entities.Base;

namespace Application.Interfaces.Persistance
{
    public interface IRepository<T>
        where T : CosmosDbEntity
    {
        Task<IEnumerable<T>> GetItemsAsync(ISpecification<T> specification);

        Task<int> GetItemsCountAsync(ISpecification<T> specification);

        Task<T> GetItemAsync(string id);

        Task<T> GetFirstItemAsync(ISpecification<T> specification);

        Task AddItemAsync(T item);

        Task UpdateItemAsync(string id, T item);

        Task DeleteItemAsync(string id);
    }
}
