using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Storage.Internal;

public class SqlServerVectorSearchTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => string.Equals(mappingInfo.StoreTypeNameBase, "vector", StringComparison.OrdinalIgnoreCase)
            ? new SqlServerVectorTypeMapping(
                mappingInfo.Size ?? throw new ArgumentException("'vector' store type must have a dimensions facet"))
            : null;
}
