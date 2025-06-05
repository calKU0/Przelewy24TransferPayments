using Przelewy24TransferPayments.Interfaces;
using Przelewy24TransferPayments.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> IsTransactionTransfered(long orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"SELECT 1 FROM [kkur].[Przelewy24Logs] WHERE OrderId = @OrderId AND Success = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }
    }
}
