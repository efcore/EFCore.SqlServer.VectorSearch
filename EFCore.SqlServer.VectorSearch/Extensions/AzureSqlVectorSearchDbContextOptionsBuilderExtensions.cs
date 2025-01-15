using EFCore.SqlServer.VectorSearch.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension method for enabling Azure SQL vector search via <see cref="AzureSqlDbContextOptionsBuilder" />.
/// </summary>
public static class AzureSqlVectorSearchDbContextOptionsBuilderExtensions
{
    /// <summary>
    ///     Adds Azure SQL vector search functionality to Entity Framework Core.
    /// </summary>
    /// <returns> The options builder so that further configuration can be chained. </returns>
    public static AzureSqlDbContextOptionsBuilder UseVectorSearch(this AzureSqlDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<SqlServerVectorSearchOptionsExtension>()
                        ?? new SqlServerVectorSearchOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
