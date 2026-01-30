using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Windows;

namespace EP_0201_MW.Helpers
{
    public static class ConnectionManager
    {
        // Файл будет лежать рядом с .exe
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "connection.txt");

        // Дефолтная строка, если файл пуст
        private const string DefaultConnection = @"Data Source=.\SQLEXPRESS;Initial Catalog=MasterSkladDB;Integrated Security=True;Trust Server Certificate=True";

        public static string GetConnectionString()
        {
            if (File.Exists(ConfigPath))
            {
                string saved = File.ReadAllText(ConfigPath).Trim();
                if (!string.IsNullOrEmpty(saved)) return saved;
            }
            return DefaultConnection;
        }

        public static void SaveConnectionString(string connectionString)
        {
            File.WriteAllText(ConfigPath, connectionString);
        }
    }
}
