using EFCore.SqlServer.VectorSearch.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class SqlServerVectorSearchPropertyBuilderExtensions
{
    public static PropertyBuilder IsVector(this PropertyBuilder propertyBuilder)
    {
        propertyBuilder
            .HasConversion<VectorValueConverter>()
            .Metadata.SetValueComparer(typeof(VectorValueComparer));
        return propertyBuilder;
    }

    public static PropertyBuilder<T> IsVector<T>(this PropertyBuilder<T> propertyBuilder)
        => (PropertyBuilder<T>)IsVector((PropertyBuilder)propertyBuilder);
}