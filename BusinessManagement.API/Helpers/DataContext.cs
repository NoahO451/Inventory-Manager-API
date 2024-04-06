using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace App.Helpers
{
    public class DataContext
    {
        private DbSettings _dbSettings;

        public DataContext(IOptions<DbSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = $"Host={_dbSettings.Server}; Database={_dbSettings.Database}; Username={_dbSettings.UserId}; Password={_dbSettings.Password};";
            return new NpgsqlConnection(connectionString);
        }

        public async Task Init()
        {
            // todo
            //await _initDatabase();
            //await _initTables();
        }
    }
}
