using Ardalis.Specification;
using Domain.Entities.Base;

namespace Infrastructure.Core.CosmosDbData
{
    public class CosmosDbSpecificationEvaluator<T> : SpecificationEvaluatorBase<T>
        where T : CosmosDbEntity
    {
    }
}
