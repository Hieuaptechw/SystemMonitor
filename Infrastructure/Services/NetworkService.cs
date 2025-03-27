using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Helpers;
using Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class NetworkService : INetworkService
    {
        private readonly IAppConfiguration _config;
     
        public NetworkService(IAppConfiguration config)
        {
            _config = config;
          
        }
        public async Task<NetworkInfo> GetNetworkInfoAsync()
        {
            try
            {
                var ipAddress = await GetLocalIPAddressAsync();
                var openPorts = await GetOpenPortsAsync();
                var databases = await GetAllDatabasesAsync();

                var networkInfo = new NetworkInfo
                {
                    IPAddress = ipAddress,
                    OpenPorts = openPorts,
                    Databases = databases,
                    RecordedAt = DateTime.UtcNow
                };

                var logMessage = $"Network information retrieved successfully. IP Address: {ipAddress}, Open Ports: {string.Join(", ", openPorts)}, Databases: {string.Join(", ", databases.SelectMany(d => d.Value))}";

                await LogService.LogInfo( logMessage);

                return networkInfo;
            }
            catch (Exception ex)
            {
                await LogService.LogError($"Error retrieving network information: {ex.Message}");
                throw;
            }
        }

        private async Task<string> GetLocalIPAddressAsync()
        {
            return await Task.Run(() =>
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                throw new Exception("Không tìm thấy địa chỉ IPv4!");
            });
        }

        private async Task<List<int>> GetOpenPortsAsync()
        {
            return await Task.Run(() =>
            {
                var openPorts = new List<int>();
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var endPoints = properties.GetActiveTcpListeners();
                foreach (var endPoint in endPoints)
                {
                    openPorts.Add(endPoint.Port);
                }
                return openPorts;
            });
        }


        public async Task<Dictionary<DatabaseType, List<string>>> GetAllDatabasesAsync()
        {
            var sqlServerConnectionString = _config.GetConnectionString("SqlServer");
            var mySqlConnectionString = _config.GetConnectionString("MySql");
            var mongoDbConnectionString = _config.GetConnectionString("MongoDb");

            var sqlServerTask = DatabaseHelper.GetSqlServerDatabasesAsync(sqlServerConnectionString);
            var mySqlTask = DatabaseHelper.GetMySqlDatabasesAsync(mySqlConnectionString);
          var mongoDbTask = DatabaseHelper.GetMongoDatabasesAsync(mongoDbConnectionString);

            await Task.WhenAll(sqlServerTask, mySqlTask, mongoDbTask);

            var result = new Dictionary<DatabaseType, List<string>>
            {
                { DatabaseType.SqlServer, sqlServerTask.Result },
                { DatabaseType.MySql, mySqlTask.Result },
                { DatabaseType.MongoDb, mongoDbTask.Result }
            };

            return result;
        }


    }
}
