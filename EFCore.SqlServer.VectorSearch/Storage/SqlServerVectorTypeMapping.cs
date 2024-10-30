using System.Buffers.Binary;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Storage.Internal;

public class SqlServerVectorTypeMapping : RelationalTypeMapping
{
    public SqlServerVectorTypeMapping(int dimensions)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(float[]), ConverterInstance, ComparerInstance, jsonValueReaderWriter: JsonByteArrayReaderWriter.Instance /* TODO */),
                "vector",
                StoreTypePostfix.Size,
                System.Data.DbType.String,
                size: dimensions))
    {
    }

    protected SqlServerVectorTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerVectorTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Note: we assume that the string argument has gone through the value converter below, and so doesn't need
    // escaping.
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"CAST('{(string)value}' AS vector({Size}))";

    private static readonly VectorComparer ComparerInstance = new();
    private static readonly VectorConverter ConverterInstance = new();

    private class VectorComparer() : ValueComparer<float[]>(
        (x, y) => x == null ? y == null : y != null && x.SequenceEqual(y),
        v => v.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode())),
        v => v.ToArray());

    private class VectorConverter() : ValueConverter<float[], string>(
        f => JsonSerializer.Serialize(f, JsonSerializerOptions.Default),
        s => JsonSerializer.Deserialize<float[]>(s, JsonSerializerOptions.Default)!);
}
