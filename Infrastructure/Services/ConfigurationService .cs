using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Application.Interfaces;

namespace Infrastructure.Services
{
    public class ConfigurationService : IAppConfiguration
    {
        private readonly Dictionary<string, string> _configCache;
        private readonly string _defaultConnectionString;

        public ConfigurationService(IConfiguration configuration)
        {
            _configCache = new Dictionary<string, string>();

            // Lưu tất cả các giá trị cấu hình vào Dictionary khi service được khởi tạo
            foreach (var section in configuration.AsEnumerable())
            {
                if (!string.IsNullOrEmpty(section.Value))
                {
                    _configCache[section.Key] = section.Value;
                }
            }

            // Lưu chuỗi kết nối mặc định
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public string this[string key] => _configCache.TryGetValue(key, out var value) ? value : string.Empty;

        public string GetConnectionString(string name)
        {
            return name == "DefaultConnection" ? _defaultConnectionString :
                   (_configCache.TryGetValue($"ConnectionStrings:{name}", out var conn) ? conn : string.Empty);
        }

        public void Reload()
        {
            throw new NotImplementedException("Reload chưa được hỗ trợ.");
        }
    }
}
