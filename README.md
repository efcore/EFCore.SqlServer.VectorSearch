# EFCore.SqlServer.VectorSearch

> [!IMPORTANT]
> Starting EF 10, the SQL Server provider contains full, built-in support for vector search. This plugin is no longer needed for EF 10 and above, and will be deprecated.
>
> For earlier versions of EF, note that this plugin is in prerelease status and not officially supported.

This Entity Framework Core plugin provides integration between older versions of EF and [Vector Support in Azure SQL Database and SQL Server 2025](https://devblogs.microsoft.com/azure-sql/announcing-general-availability-of-native-vector-type-functions-in-azure-sql/), allowing LINQ to be used to perform vector similarity search, and seamless insertion/retrieval of vector data.

To use the plugin, reference the [EFCore.SqlServer.VectorSearch](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch) nuget package, and enable the plugin by adding `UseVectorSearch()` to your `UseSqlServer()` or `UseAzureSql()` config as follows:

```c#
builder.Services.AddDbContext<ProductContext>(options =>
  options.UseSqlServer("<connection string>", o => o.UseVectorSearch()));
```

Once the plugin has been enabled, add an ordinary `float[]` property to the .NET type being mapped with EF:

```c#
public class Product
{
    public int Id { get; set; }
    public float[] Embedding { get; set; }
}
```

Finally, configure the property to be mapped as a vector by letting EF Core know using the `HasColumnType` method. Use the `vector` type and specify the number of dimension that your vector will have:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().Property(p => p.Embedding).HasColumnType("vector(3)");
}
```

That's it - you can now perform similarity search in LINQ queries! For example, to get the top 5 most similar products:

```c#
var someVector = new[] { 1f, 2f, 3f };
var products = await context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, someVector))
    .Take(5)
    .ToArrayAsync();
```

A full sample using EF Core and vectors is available here:

> [!NOTE]
> The [`VECTOR_SEARCH()`](/sql/t-sql/functions/vector-search-transact-sql) function (in preview) for approximate search with DiskANN is not supported.

[Azure SQL DB Vector Samples - EF-Core Sample](https://github.com/Azure-Samples/azure-sql-db-vector-search/tree/main/DotNet/EF-Core)

Ideas? Issues? Let us know on the [issues page](https://github.com/efcore/EFCore.SqlServer.VectorSearch/issues).
