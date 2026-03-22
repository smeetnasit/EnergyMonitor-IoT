using Microsoft.Data.SqlClient;
using System.Data;

namespace EnergyMonitor.Simulation
{
    public class DBContext
    {
        private readonly string _connectionString;

        public DBContext(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}