using EFCore.SqlServer.VectorSearch.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension method for enabling Azure SQL vector search via <see cref="SqlServerDbContextOptionsBuilder" />.
/// </summary>
public static class SqlServerVectorSearchDbContextOptionsBuilderExtensions
{
    /// <summary>
    ///     Adds Azure SQL vector search functionality to Entity Framework.
    /// </summary>
    /// <returns> The options builder so that further configuration can be chained. </returns>
    public static SqlServerDbContextOptionsBuilder UseVectorSearch(this SqlServerDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<SqlServerVectorSearchOptionsExtension>()
                        ?? new SqlServerVectorSearchOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
