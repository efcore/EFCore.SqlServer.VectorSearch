using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Storage.Internal;

public class SqlServerVectorSearchTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => mappingInfo.StoreTypeNameBase is "vector"
            ? new SqlServerVectorTypeMapping(
                mappingInfo.Size ?? throw new ArgumentException("'vector' store type must have a dimensions facet"))
            : null;
}