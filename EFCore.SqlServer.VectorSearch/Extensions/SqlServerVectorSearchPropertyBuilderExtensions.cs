using EFCore.SqlServer.VectorSearch.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class SqlServerVectorSearchPropertyBuilderExtensions
{
    [Obsolete("Configure your property to use the 'vector' via HasColumnType()")]
    public static PropertyBuilder IsVector(this PropertyBuilder propertyBuilder)
        => throw new NotSupportedException();

    [Obsolete("Configure your property to use the 'vector' via HasColumnType()")]
    public static PropertyBuilder<T> IsVector<T>(this PropertyBuilder<T> propertyBuilder)
        => throw new NotSupportedException();
}