using MaintenanceApp.Mapping.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceApp.Singletone
{
    public class SqliteDbContext
    {

        private static readonly Lazy<SqliteDbContext> lazy = new Lazy<SqliteDbContext>(() => new SqliteDbContext());
        public static SqliteDbContext Instance => lazy.Value;


        private const string DatabaseFileName = "sqlitedatabase.db";
        private static readonly string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private SQLiteAsyncConnection _database;

        private async Task InitAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            await _database.CreateTableAsync<Connection>();

            // Check if the table is empty before inserting the default values
            var existingConnection = await _database.Table<Connection>().FirstOrDefaultAsync();
            if (existingConnection == null)
            {
                // Insert the default IP and Port values
                var defaultConnection = new Connection
                {
                    Id = 1,
                    Url = "http://10.0.2.2:120",
                };
                await _database.InsertAsync(defaultConnection);
            }
        }
        public async Task<Connection> GetConnectionAsync()
        {
            await InitAsync();
            return await _database.Table<Connection>().FirstOrDefaultAsync();
        }
        // Function to update the Ip and Port values
        public async Task<bool> UpdateConnectionAsync(string newUrl)
        {
            await InitAsync();

            var connection = await _database.Table<Connection>().FirstOrDefaultAsync();
            if (connection != null)
            {
                connection.Url = newUrl;
                await _database.UpdateAsync(connection);
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
