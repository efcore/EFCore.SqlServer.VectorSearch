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
ORDER BY VECTOR_DISTANCE('cosine', [p].[Embedding], CAST('[1,2,3]' AS vector(3)))
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
@__vector_1='[1,2,3]' (Size = 7)

SELECT TOP(@__p_2) [p].[Id], [p].[Embedding]
FROM [Products] AS [p]
ORDER BY VECTOR_DISTANCE('dot', [p].[Embedding], CAST(@__vector_1 AS vector(3)))
""");
    }

    [ConditionalFact]
    public virtual async Task Select_vector_out()
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

        [Vector(3)]
        public float[] Embedding { get; set; }
    }

    protected VectorSearchContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
