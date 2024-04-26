using Microsoft.EntityFrameworkCore.ChangeTracking;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Storage.Internal;

public class VectorValueComparer() : ValueComparer<float[]>(
    (x, y) => x == null ? y == null : y != null && x.SequenceEqual(y),
    v => v.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode())),
    v => v.ToArray());
