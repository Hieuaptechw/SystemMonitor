using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MongoDB.Driver;

namespace Infrastructure.Helpers
{
    public static class DatabaseHelper
    {
        public static async Task<List<string>> GetSqlServerDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT name FROM sys.databases", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            databases.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return databases;
        }

        public static async Task<List<string>> GetMySqlDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SHOW DATABASES;", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            databases.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return databases;
        }

        public static async Task<List<string>> GetMongoDatabasesAsync(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var databases = await client.ListDatabaseNamesAsync();
            return await databases.ToListAsync();
        }
    }
}
