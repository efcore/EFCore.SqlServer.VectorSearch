using System.Reflection;
using EFCore.SqlServer.VectorSearch.Storage.Internal;
using Microsoft.Data.SqlClient;
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
    // private readonly RelationalTypeMapping _vectorTypeMapping;

    public SqlServerVectorSearchMethodCallTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
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

                // At least one of the two arguments must be a vector (i.e. SqlServerVectorTypeMapping).
                // Check this and extract the mapping, applying it to the other argument (e.g. in case it's a parameter).
                var vectorMapping =
                    arguments[2].TypeMapping as SqlServerVectorTypeMapping
                    ?? arguments[3].TypeMapping as SqlServerVectorTypeMapping
                    ?? throw new InvalidOperationException(
                        "At least one of the arguments to EF.Functions.VectorDistance must be a vector");

                var vector1 = Wrap(_sqlExpressionFactory.ApplyTypeMapping(arguments[2], vectorMapping), vectorMapping);
                var vector2 = Wrap(_sqlExpressionFactory.ApplyTypeMapping(arguments[3], vectorMapping), vectorMapping);

                SqlExpression Wrap(SqlExpression expression, SqlServerVectorTypeMapping vectorMapping)
                    => expression is SqlParameterExpression
                        ? _sqlExpressionFactory.Convert(expression, typeof(float[]), vectorMapping)
                        : expression;

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

            default:
                return null;
        }
    }
}
