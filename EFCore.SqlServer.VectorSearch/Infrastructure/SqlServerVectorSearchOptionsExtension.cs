using EFCore.SqlServer.VectorSearch.Query.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerVectorSearchOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkSqlServerVectorSearch();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
        if (internalServiceProvider is not null)
        {
            using var scope = internalServiceProvider.CreateScope();

            if (scope.ServiceProvider.GetService<IEnumerable<IMethodCallTranslatorPlugin>>()
                    ?.Any(s => s is SqlServerVectorSearchMethodCallTranslatorPlugin)
                != true)
            {
                throw new InvalidOperationException(
                    $"{nameof(SqlServerVectorSearchDbContextOptionsBuilderExtensions.UseVectorSearch)} requires {nameof(SqlServerVectorSearchServiceCollectionExtensions.AddEntityFrameworkSqlServerVectorSearch)} to be called on the internal service provider used.");
            }
        }
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        private new SqlServerVectorSearchOptionsExtension Extension
            => (SqlServerVectorSearchOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => false;

        public override int GetServiceProviderHashCode()
            => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => true;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["SqlServer:" + nameof(SqlServerVectorSearchDbContextOptionsBuilderExtensions.UseVectorSearch)] = "1";

        public override string LogFragment
            => "using SQL Server vector search";
    }
}
