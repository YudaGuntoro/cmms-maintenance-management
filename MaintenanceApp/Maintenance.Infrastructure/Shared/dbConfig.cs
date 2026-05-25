namespace Maintenance.Infrastructure.Shared
{
    public static class dbConfig
    {
        public static string MysqlConnString
        {
            get
            {
                var directConnectionString = GetEnvironmentValue("ConnectionStrings__DefaultConnection")
                    ?? GetEnvironmentValue("MYSQL_CONNECTION_STRING");

                if (!string.IsNullOrWhiteSpace(directConnectionString))
                {
                    return directConnectionString;
                }

                var server = GetConfigValue("MYSQL_HOST", "Server", "127.0.0.1");
                var port = GetConfigValue("MYSQL_PORT", "Port", "3306");
                var userId = GetConfigValue("MYSQL_USER", "UserID", "root");
                var password = GetConfigValue("MYSQL_PASSWORD", "Password", string.Empty);
                var database = GetConfigValue("MYSQL_DATABASE", "Db", "db_maintenance");
                var sslMode = GetEnvironmentValue("MYSQL_SSL_MODE") ?? "None";

                return "Server=" + server + ";" +
                    "Port=" + port + ";" +
                    "User ID=" + userId + ";" +
                    "Password=" + password + ";" +
                    "Database=" + database + ";" +
                    "SslMode=" + sslMode + ";" +
                    "AllowPublicKeyRetrieval=True;";
            }
        }

        private static string GetConfigValue(string environmentKey, string iniKey, string defaultValue)
        {
            return GetEnvironmentValue(environmentKey)
                ?? EmptyToNull(ConfigRead.Instance.Read(iniKey, "Connections"))
                ?? defaultValue;
        }

        private static string? GetEnvironmentValue(string key)
        {
            return EmptyToNull(Environment.GetEnvironmentVariable(key));
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
