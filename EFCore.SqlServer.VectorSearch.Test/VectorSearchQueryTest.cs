using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace EFCore.SqlServer.VectorSearch.Test;

public class VectorSearchQueryTest
    : IClassFixture<VectorSearchQueryTest.VectorSearchQueryFixture>
{
    private VectorSearchQueryFixture Fixture { get; }

    public VectorSearchQueryTest(VectorSearchQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual async Task VectorDistance_with_constant()
    {
        await using var context = CreateContext();

        _ = await context.Products
             .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, new[] { 1f, 2f, 3f }))
             .Take(1)
             .ToArrayAsync();

        AssertSql(
            """
@__p_1='1'

SELECT TOP(@__p_1) [p].[Id], [p].[Embedding]
FROM [Products] AS [p]
ORDER BY VECTOR_DISTANCE('cosine', [p].[Embedding], 0xA9010300000000000000803F0000004000004040)
""");
    }
    
    [ConditionalFact]
    public virtual async Task VectorDistance_with_parameter()
    {
        await using var context = CreateContext();

        var vector = new[] { 1f, 2f, 3f };
        _ = await context.Products
            .OrderBy(p => EF.Functions.VectorDistance("dot", p.Embedding, vector))
            .Take(1)
            .ToArrayAsync();

        AssertSql(
            """
@__p_2='1'
@__vector_1='0xA9010300000000000000803F0000004000004040' (Size = 8000)

SELECT TOP(@__p_2) [p].[Id], [p].[Embedding]
FROM [Products] AS [p]
ORDER BY VECTOR_DISTANCE('dot', [p].[Embedding], @__vector_1)
""");
    }

    [ConditionalFact]
    public virtual async Task Dimensions()
    {
        await using var context = CreateContext();

        var count = await context.Products.CountAsync(p => EF.Functions.VectorDimensions(p.Embedding) == 3);
        Assert.Equal(2, count);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE VECTOR_DIMENSIONS([p].[Embedding]) = 3
""");
    }

    [ConditionalFact]
    public virtual async Task JsonArrayToVector()
    {
        await using var context = CreateContext();

        var vector = await context.Products
            .Where(p => p.Id == 1)
            .Select(p => EF.Functions.JsonArrayToVector("[1,2,3]"))
            .SingleAsync();
        Assert.Equivalent(new float[] { 1, 2, 3 }, vector);

        AssertSql(
            """
SELECT TOP(2) JSON_ARRAY_TO_VECTOR(N'[1,2,3]')
FROM [Products] AS [p]
WHERE [p].[Id] = 1
""");
    }

    [ConditionalFact]
    public virtual async Task VectorToJsonArray()
    {
        await using var context = CreateContext();

        var jsonArray = await context.Products
            .Where(p => p.Id == 1)
            .Select(p => EF.Functions.VectorToJsonArray(p.Embedding))
            .SingleAsync();
        Assert.Equal("[1.0000000000000000,2.0000000000000000,3.0000000000000000]", jsonArray);

        AssertSql(
            """
SELECT TOP(2) VECTOR_TO_JSON_ARRAY([p].[Embedding])
FROM [Products] AS [p]
WHERE [p].[Id] = 1
""");
    }

    [ConditionalFact]
    public virtual async Task Value_converter_writes_correctly()
    {
        await using var context = CreateContext();

        float[] localVector = [1, 2, 3]; 
        var product = await context.Products.Where(p => p.Embedding == localVector).SingleAsync();
        Assert.Equal(1, product.Id);

        AssertSql(
            """
@__localVector_0='0xA9010300000000000000803F0000004000004040' (Size = 8000)

SELECT TOP(2) [p].[Id], [p].[Embedding]
FROM [Products] AS [p]
WHERE [p].[Embedding] = @__localVector_0
""");
    }

    [ConditionalFact]
    public virtual async Task Value_converter_reads_correctly()
    {
        await using var context = CreateContext();

        var serverVector = await context.Products.Where(p => p.Id == 1).Select(p => p.Embedding).SingleAsync();

        Assert.Equivalent(new float[] { 1, 2, 3 }, serverVector);
        
        AssertSql(
            """
SELECT TOP(2) [p].[Embedding]
FROM [Products] AS [p]
WHERE [p].[Id] = 1
""");
    }

    public class VectorSearchQueryFixture : SharedStoreFixtureBase<VectorSearchContext>
    {
        protected override string StoreName => "vectordb";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
        
        protected override void Seed(VectorSearchContext context)
            => VectorSearchContext.Seed(context);
    }
    
    public class VectorSearchContext(DbContextOptions<VectorSearchContext> options) : PoolableDbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity => entity.Property(e => e.Embedding).IsVector());
        }

        public static void Seed(VectorSearchContext context)
        {
            context.Products.AddRange(
                new Product { Id = 1, Embedding = [1, 2, 3] },
                new Product { Id = 2, Embedding = [10, 20, 30] });
            context.SaveChanges();
        }
    }

    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        
        public float[] Embedding { get; set; } 
    }
    
    protected VectorSearchContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}