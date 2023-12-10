using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using TP2_Interface.Models;

namespace TP2_Interface.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<KnowledgeEntry>> GetAllEntriesAsync()
    {
        var entries = new List<KnowledgeEntry>();
        
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("SELECT * FROM connaissance", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    entries.Add(new KnowledgeEntry
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Title = reader.GetString(reader.GetOrdinal("titre")),
                        Description = reader.GetString(reader.GetOrdinal("description")),
                        JsonFields = reader.GetString(reader.GetOrdinal("champs"))
                    });
                }
            }
        }

        return entries;
    }

    public async Task<List<KnowledgeEntry>> SearchEntriesAsync(string searchQuery)
    {
        var entries = new List<KnowledgeEntry>();
        var lowerSearchQuery = searchQuery.ToLower();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            // Use the LOWER function to make the search case-insensitive
            using (var command = new NpgsqlCommand(@"SELECT * FROM connaissance 
                                                 WHERE LOWER(titre) LIKE @query 
                                                 OR LOWER(description) LIKE @query 
                                                 OR champs::text LIKE @query", connection))
            {
                // Use lower-case search query
                command.Parameters.AddWithValue("@query", $"%{lowerSearchQuery}%");

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new KnowledgeEntry
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Title = reader.GetString(reader.GetOrdinal("titre")),
                            Description = reader.GetString(reader.GetOrdinal("description")),
                            // Make sure to handle the JSON data properly
                            JsonFields = reader.GetString(reader.GetOrdinal("champs"))
                        });
                    }
                }
            }
        }

        return entries;
    }
    
    public async Task<List<KnowledgeEntry>> AdvancedSearchEntriesAsync(string searchField, string searchQuery)
    {
        var entries = new List<KnowledgeEntry>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(@"SELECT * FROM connaissance 
                                                 WHERE champs ->> @searchField ILIKE @query", connection))
            {
                command.Parameters.AddWithValue("@searchField", searchField);
                command.Parameters.AddWithValue("@query", $"%{searchQuery}%");

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new KnowledgeEntry
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Title = reader.GetString(reader.GetOrdinal("titre")),
                            Description = reader.GetString(reader.GetOrdinal("description")),
                            JsonFields = reader.GetString(reader.GetOrdinal("champs"))
                        });
                    }
                }
            }
        }

        return entries;
    }


    
    public async Task CreateEntryAsync(KnowledgeEntry entry)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(@"INSERT INTO connaissance (titre, description, champs) 
                                                 VALUES (@title, @description, @jsonFields::jsonb)", connection))
            {
                command.Parameters.AddWithValue("@title", entry.Title);
                command.Parameters.AddWithValue("@description", entry.Description);
                var jsonParam = new NpgsqlParameter("@jsonFields", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = entry.JsonFields };
                command.Parameters.Add(jsonParam);

                await command.ExecuteNonQueryAsync();
            }
        }
    }


    public async Task UpdateEntryAsync(KnowledgeEntry entry)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(@"UPDATE connaissance 
                                                SET titre = @title, 
                                                    description = @description, 
                                                    champs = @jsonFields 
                                                WHERE id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", entry.Id);
                command.Parameters.AddWithValue("@title", entry.Title);
                command.Parameters.AddWithValue("@description", entry.Description);
            
                // Explicitly set the type to jsonb for the jsonFields parameter
                var jsonbParam = new NpgsqlParameter("@jsonFields", NpgsqlDbType.Jsonb) { Value = entry.JsonFields };
                command.Parameters.Add(jsonbParam);

                await command.ExecuteNonQueryAsync();
            }
        }
    }



    public async Task DeleteEntryAsync(int entryId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(@"DELETE FROM connaissance WHERE id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", entryId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    
}
