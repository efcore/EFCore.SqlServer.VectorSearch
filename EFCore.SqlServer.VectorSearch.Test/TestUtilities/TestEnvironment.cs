// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.TestUtilities;

// Copied from EF

public static class TestEnvironment
{
    public static IConfiguration Config { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json", optional: true)
        .AddJsonFile("config.test.json", optional: true)
        .AddEnvironmentVariables()
        .Build()
        .GetSection("Test:SqlServer");

    public static string DefaultConnection { get; } = "Server=tcp:shaysqltestserver.database.windows.net,1433;Initial Catalog=shaytest;Persist Security Info=False;User ID=roji;Password=Abcd5678;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False";

    private static readonly string _dataSource = new SqlConnectionStringBuilder(DefaultConnection).DataSource;

    public static bool IsConfigured { get; } = !string.IsNullOrEmpty(_dataSource);

    public static bool IsCI { get; } = Environment.GetEnvironmentVariable("CI") != null
        || Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null;

    private static bool? _isAzureSqlDb;

    private static bool? _supportsFunctions2017;

    private static bool? _supportsFunctions2019;

    private static bool? _supportsFunctions2022;

    private static byte? _productMajorVersion;

    private static int? _engineEdition;

    public static bool IsSqlAzure
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_isAzureSqlDb.HasValue)
            {
                return _isAzureSqlDb.Value;
            }

            try
            {
                _isAzureSqlDb = GetEngineEdition() is 5 or 8;
            }
            catch (PlatformNotSupportedException)
            {
                _isAzureSqlDb = false;
            }

            return _isAzureSqlDb.Value;
        }
    }

    public static bool IsLocalDb { get; } = _dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

    public static bool IsFunctions2017Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsFunctions2017.HasValue)
            {
                return _supportsFunctions2017.Value;
            }

            try
            {
                _supportsFunctions2017 = GetProductMajorVersion() >= 14 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsFunctions2017 = false;
            }

            return _supportsFunctions2017.Value;
        }
    }

    public static bool IsFunctions2019Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsFunctions2019.HasValue)
            {
                return _supportsFunctions2019.Value;
            }

            try
            {
                _supportsFunctions2019 = GetProductMajorVersion() >= 15 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsFunctions2019 = false;
            }

            return _supportsFunctions2019.Value;
        }
    }

    public static bool IsFunctions2022Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsFunctions2022.HasValue)
            {
                return _supportsFunctions2022.Value;
            }

            try
            {
                _supportsFunctions2022 = GetProductMajorVersion() >= 16 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsFunctions2022 = false;
            }

            return _supportsFunctions2022.Value;
        }
    }

    public static byte SqlServerMajorVersion
        => GetProductMajorVersion();

    public static string? ElasticPoolName { get; } = Config["ElasticPoolName"];

    public static bool? GetFlag(string key)
        => bool.TryParse(Config[key], out var flag) ? flag : null;

    public static int? GetInt(string key)
        => int.TryParse(Config[key], out var value) ? value : null;

    private static int GetEngineEdition()
    {
        if (_engineEdition.HasValue)
        {
            return _engineEdition.Value;
        }

        using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
        sqlConnection.Open();

        using var command = new SqlCommand(
            "SELECT SERVERPROPERTY('EngineEdition');", sqlConnection);
        _engineEdition = (int)command.ExecuteScalar();

        return _engineEdition.Value;
    }

    private static byte GetProductMajorVersion()
    {
        if (_productMajorVersion.HasValue)
        {
            return _productMajorVersion.Value;
        }

        using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
        sqlConnection.Open();

        using var command = new SqlCommand(
            "SELECT SERVERPROPERTY('ProductVersion');", sqlConnection);
        _productMajorVersion = (byte)Version.Parse((string)command.ExecuteScalar()).Major;

        return _productMajorVersion.Value;
    }
}
