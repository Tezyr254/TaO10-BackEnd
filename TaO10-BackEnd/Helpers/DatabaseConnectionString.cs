using System.Net;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TaO10_BackEnd.Helpers;

public static class DatabaseConnectionString
{
    public static string Get(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MyCnn")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return IsPostgresUrl(connectionString)
                ? FromPostgresUrl(connectionString)
                : connectionString;
        }

        var databaseUrl = configuration["DATABASE_URL"] ?? configuration["POSTGRES_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return FromPostgresUrl(databaseUrl);
        }

        throw new InvalidOperationException(
            "Missing database connection. Set ConnectionStrings__MyCnn, ConnectionStrings__DefaultConnection, or DATABASE_URL.");
    }

    private static bool IsPostgresUrl(string value)
    {
        return value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase);
    }

    private static string FromPostgresUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var credentials = uri.UserInfo.Split(':', 2);

        if (credentials.Length != 2)
        {
            throw new InvalidOperationException("DATABASE_URL must include username and password.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = WebUtility.UrlDecode(credentials[0]),
            Password = WebUtility.UrlDecode(credentials[1]),
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}
