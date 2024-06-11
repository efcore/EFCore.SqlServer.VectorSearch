# EFCore.SqlServer.VectorSearch

> [!IMPORTANT]  
> This plugin is in prerelease status, and the APIs described below are likely to change before the final release.
> Usage of this plugin requires the vector support feature in Azure SQL Database, currently in EAP. [See this blog post](https://devblogs.microsoft.com/azure-sql/announcing-eap-native-vector-support-in-azure-sql-database/) for more details.

This Entity Framework Core plugin provides integration between EF and Vector Support in Azure SQL Database, allowing LINQ to be used to perform vector similarity search, and seamless insertion/retrieval of vector data.

To use the plugin, reference the [EFCore.SqlServer.VectorSearch](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch) nuget package, and enable the plugin by adding `UseVectorSearch()` to your `UseSqlServer()` config as follows:

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

Finally, configure the property to be mapped as a vector by applying `IsVector()`:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().Property(p => p.Embedding).IsVector();
}
```

That's it - you can now perform similarity search in LINQ queries! For example, to get the top 5 most similar products:

```c#
var someVector = new[] { 1f, 2f, 3f };
var products = await context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vector))
    .Take(5)
    .ToArrayAsync();
```

To get the number of dimensions of a vector, use `EF.Functions.VectorDimensions()`:

```c#
var dimensions = await context.Products
    .Where(p => p.Id == 1)
    .Select(p => EF.Functions.VectorDimensions(p.Embedding))
    .SingleAsync();
```

Finally, the `JsonArrayToVector()` and `VectorToJsonArray()` functions allow you to convert a `varchar` value containing
a JSON array to a vector, and vice versa. These functions generally shouldn't be needed, as you can work with `float[]`
directly, and the EF plugin will perform the conversions for you.

Ideas? Issues? Let us know on the [github repo](https://github.com/efcore/EFCore.SqlServer.VectorSearch).
