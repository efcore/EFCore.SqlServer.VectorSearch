// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.TestUtilities;

// Copied from EF

public class SqlServerTestStoreFactory : RelationalTestStoreFactory
{
    public static SqlServerTestStoreFactory Instance { get; } = new();

    protected SqlServerTestStoreFactory()
    {
    }

    public override TestStore Create(string storeName)
        => SqlServerTestStore.Create(storeName);

    public override TestStore GetOrCreate(string storeName)
        => SqlServerTestStore.GetOrCreate(storeName);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddEntityFrameworkSqlServer()
            .AddEntityFrameworkSqlServerVectorSearch();
}
