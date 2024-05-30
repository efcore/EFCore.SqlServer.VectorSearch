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

    /// <summary>
    ///     Returns the number of dimensions for the vector.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="vector1">The first vector.</param>
    public static int VectorDimensions(this DbFunctions _, float[] vector1)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDimensions)));

    /// <summary>
    ///     Converts the given JSON array into a vector.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="jsonArray">The JSON array to convert.</param>
    public static float[] JsonArrayToVector(this DbFunctions _, string jsonArray)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonArrayToVector)));

    /// <summary>
    ///     Converts the vector into a JSON array.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="vector">The vector to convert.</param>
    public static string VectorToJsonArray(this DbFunctions _, float[] vector)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorToJsonArray)));

    /// <summary>
    ///     Returns whether the given binary data represents a vector.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="binaryData">The binary data.</param>
    public static bool IsVector(this DbFunctions _, byte[] binaryData)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsVector)));
}