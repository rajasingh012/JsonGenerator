using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NanoidDotNet;
using Npgsql;

public class MockSchemaService
{
    private readonly string _connectionString;

    public MockSchemaService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres");
    }

    public async Task<string> SaveSchemaAsync(string jsonSchema)
    {
        // Generate a short, random ID (default: 21 characters, URL-friendly)
        var id = await Nanoid.GenerateAsync(); // You can change size if needed

        const string sql = @"
        INSERT INTO mock_sessions (id, schema, created_at)
        VALUES (@id, @schema::jsonb, NOW())";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("schema", NpgsqlTypes.NpgsqlDbType.Jsonb, jsonSchema);

        await cmd.ExecuteNonQueryAsync();

        return id;
    }

    public async Task<string?> GetSchemaAsync(string shortUrl)
    {
        const string sql = @"
        SELECT schema::text
        FROM mock_sessions
        WHERE id = @id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", shortUrl); 

        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

}
