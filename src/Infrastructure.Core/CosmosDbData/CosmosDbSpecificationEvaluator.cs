using Ardalis.Specification;

namespace Infrastructure.Core.CosmosDbData
{
    /// <summary>
    ///     Specification Evaluator for Cosmos DB.
    ///     The evaluator implements methods to translate specifications into Cosmos DB IQueryables, which then allows us to build queryables with filters, predicates etc. to query data.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class CosmosDbSpecificationEvaluator<T> : SpecificationEvaluatorBase<T>
        where T : class
    {
    }
}
