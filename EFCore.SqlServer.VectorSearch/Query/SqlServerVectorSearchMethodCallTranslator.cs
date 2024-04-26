using System.Reflection;
using EFCore.SqlServer.VectorSearch.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Query.Internal;

public class SqlServerVectorSearchMethodCallTranslator : IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _vectorTypeMapping;

    public SqlServerVectorSearchMethodCallTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;

        _vectorTypeMapping = (RelationalTypeMapping)_typeMappingSource.FindMapping(typeof(byte[]))!
            .WithComposedConverter(new VectorValueConverter(), new VectorValueComparer());
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(SqlServerVectorSearchDbFunctionsExtensions))
        {
            return null;
        }

        switch (method.Name)
        {
            case nameof(SqlServerVectorSearchDbFunctionsExtensions.VectorDistance):
                if (arguments[1] is not SqlConstantExpression { Value: string similarityMeasure })
                {
                    throw new InvalidOperationException(
                        "The first argument to EF.Functions.VectorDistance must be a constant string");
                }

                // At least one of the two arguments must be a vector (i.e. a type mapping with a VectorValueConverter).
                // Check this and extract the mapping, applying it to the other argument (e.g. in case it's a parameter).
                var vectorMapping = arguments[2].TypeMapping is { Converter: VectorValueConverter } mapping1
                    ? mapping1
                    : arguments[3].TypeMapping is { Converter: VectorValueConverter } mapping2
                        ? mapping2
                        : throw new InvalidOperationException(
                            "At least one of the arguments to EF.Functions.VectorDistance must be a vector");

                var vector1 = _sqlExpressionFactory.ApplyTypeMapping(arguments[2], vectorMapping);
                var vector2 = _sqlExpressionFactory.ApplyTypeMapping(arguments[3], vectorMapping);
            
                return _sqlExpressionFactory.Function(
                    "VECTOR_DISTANCE",
                    [
                        _sqlExpressionFactory.Constant(similarityMeasure, _typeMappingSource.FindMapping("varchar(max)")),
                        vector1,
                        vector2
                    ],
                    nullable: true,
                    [true, true, true],
                    typeof(double),
                    _typeMappingSource.FindMapping(typeof(double)));

            case nameof(SqlServerVectorSearchDbFunctionsExtensions.VectorDimensions):
                return _sqlExpressionFactory.Function(
                    "VECTOR_DIMENSIONS",
                    [arguments[1]],
                    nullable: true,
                    [true],
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int)));

            case nameof(SqlServerVectorSearchDbFunctionsExtensions.JsonArrayToVector):
                return _sqlExpressionFactory.Function(
                    "JSON_ARRAY_TO_VECTOR",
                    [arguments[1]],
                    nullable: true,
                    [true],
                    typeof(float[]),
                    _vectorTypeMapping);

            case nameof(SqlServerVectorSearchDbFunctionsExtensions.VectorToJsonArray):
                return _sqlExpressionFactory.Function(
                    "VECTOR_TO_JSON_ARRAY",
                    [arguments[1]],
                    nullable: true,
                    [true],
                    typeof(string),
                    _typeMappingSource.FindMapping(typeof(string)));

            case nameof(SqlServerVectorSearchDbFunctionsExtensions.IsVector):
                return _sqlExpressionFactory.Function(
                    "ISVECTOR",
                    [arguments[1]],
                    nullable: true,
                    [true],
                    typeof(bool),
                    _typeMappingSource.FindMapping(typeof(bool)));

            default:
                return null;
        }
    }
}