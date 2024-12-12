using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

// Copied from EF

public static class SqlServerDbContextOptionsBuilderExtensions
{
    public static SqlServerDbContextOptionsBuilder ApplyConfiguration(this SqlServerDbContextOptionsBuilder optionsBuilder)
    {
        var maxBatch = TestEnvironment.GetInt(nameof(SqlServerDbContextOptionsBuilder.MaxBatchSize));
        if (maxBatch.HasValue)
        {
            optionsBuilder.MaxBatchSize(maxBatch.Value);
        }

        optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

        optionsBuilder.ExecutionStrategy(d => new TestSqlServerRetryingExecutionStrategy(d));

        optionsBuilder.CommandTimeout(SqlServerTestStore.CommandTimeout);

        return optionsBuilder;
    }
}
