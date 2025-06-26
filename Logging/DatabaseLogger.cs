using Przelewy24TransferPayments.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Logging
{
    public static class DatabaseLogger
    {
        public static async Task LogTransaction(long orderId, string sessionId, int merchantId, long amount, string error = null)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ToString();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"INSERT INTO [kkur].[Przelewy24Logs]
                    ([OrderId], [SessionId], [Amount], [Currency], [MerchantId], [Success], [ErrorMessage])
                    VALUES (@OrderId, @SessionId, @Amount, @Currency, @MerchantId, @Success, @ErrorMessage)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@Currency", "PLN");
                    command.Parameters.AddWithValue("@MerchantId", merchantId);
                    command.Parameters.AddWithValue("@Success", string.IsNullOrEmpty(error) ? 1 : 0);
                    command.Parameters.AddWithValue("@ErrorMessage", string.IsNullOrEmpty(error) ? (object)DBNull.Value : error);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
