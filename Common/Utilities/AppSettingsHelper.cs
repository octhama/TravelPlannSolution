using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    /// <summary>
    /// Helper class to retrieve settings from the appsettings config file
    /// </summary>
    public class AppSettingsHelper
    {
        /// <summary>
        /// Get app setting value based on his key
        /// </summary>
        /// <param name="settingKey"></param>
        /// <returns></returns>
        public static string GetSettingValue(string settingKey)
        {
            var config = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", true, true)
                                    .Build();
            return config[settingKey];
        }

        /// <summary>
        /// Get a connection string from his name
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionStringName)
        {
            string connectionStringKey = $"ConnectionStrings:{connectionStringName}";
            return GetSettingValue(connectionStringKey);
        }
    }

}
