using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MongoDB.Driver;
using Infrastructure.Logging;

namespace Infrastructure.Helpers
{
    public  class DatabaseHelper
    {
        private readonly LogService _logService;
        public DatabaseHelper(LogService logService)
        {
            _logService = logService;
        }
        public static async Task<List<string>> GetSqlServerDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();

            try
            {
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
            }
            catch (Exception ex)
            {
                await LogService.LogError($"Error fetching SQL Server databases: {ex.Message}");
              
            }

            return databases;
        }

        public static async Task<List<string>> GetMySqlDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();

            try
            {
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
            }
            catch (Exception ex)
            {
                await LogService.LogError($"Error fetching MySQL databases: {ex.Message}");
                
            }

            return databases;
        }

        public static async Task<List<string>> GetMongoDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();

            try
            {
                var client = new MongoClient(connectionString);
                var dbList = await client.ListDatabaseNamesAsync();
                databases = await dbList.ToListAsync();
            }
            catch (Exception ex)
            {
                await LogService.LogError($"Error fetching MongoDB databases: {ex.Message}");
            }

            return databases;
        }
    }
}
