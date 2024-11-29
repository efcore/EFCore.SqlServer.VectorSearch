using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class SqlServerVectorSearchPropertyBuilderExtensions
{
    public static PropertyBuilder IsVector(this PropertyBuilder propertyBuilder, int size = 1536)
        => propertyBuilder.HasColumnType($"vector({size})");

    public static PropertyBuilder<T> IsVector<T>(this PropertyBuilder<T> propertyBuilder, int size = 1536)
        => propertyBuilder.HasColumnType($"vector({size})");
}
