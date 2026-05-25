using Maintenance.Infrastructure.Shared;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Infrastructure.Context
{
    public class DapperContext
    {
        /// <summary>
        /// Connection to Database 
        /// </summary>
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? dbConfig.MysqlConnString;
        }
        public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
    }
}
