using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class SqlServerVectorSearchDbFunctionsExtensions
{
    /// <summary>
    ///     Returns the distance between two vectors, given a similarity measure.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="similarityMeasure">
    /// The similarity measure to use; can be <c>dot</c>, <c>cosine</c>, or <c>euclidean</c>.
    /// </param>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    public static double VectorDistance(
        this DbFunctions _,
        [NotParameterized] string similarityMeasure,
        float[] vector1,
        float[] vector2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));
}