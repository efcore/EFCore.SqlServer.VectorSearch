using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Query.Internal;

public class SqlServerVectorSearchEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
{
    public bool IsEvaluatableExpression(Expression expression)
        => expression switch
        {
            MethodCallExpression methodCallExpression 
                when methodCallExpression.Method.DeclaringType == typeof(SqlServerVectorSearchDbFunctionsExtensions)
                => false,
            
            _ => true
        };
}