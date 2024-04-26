using EFCore.SqlServer.VectorSearch.Query.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class SqlServerVectorSearchServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services required for Azure SQL vector search support for Entity Framework.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEntityFrameworkSqlServerVectorSearch(
        this IServiceCollection serviceCollection)
    {
        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IMethodCallTranslatorPlugin, SqlServerVectorSearchMethodCallTranslatorPlugin>()
            .TryAdd<IEvaluatableExpressionFilterPlugin, SqlServerVectorSearchEvaluatableExpressionFilterPlugin>();

        return serviceCollection;
    }
}