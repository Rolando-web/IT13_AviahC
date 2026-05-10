using Microsoft.Data.SqlClient;
using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Base database service with core connection and query execution methods.
    /// Domain-specific methods are in partial class files:
    ///   - DatabaseService.Auth.cs       (Login/User lookup)
    ///   - DatabaseService.Orders.cs     (Orders + Deliveries)
    ///   - DatabaseService.Inventory.cs  (Products CRUD)
    ///   - DatabaseService.Admin.cs      (Customers, Suppliers, Sales, Production, Promotions)
    ///   - DatabaseService.Feedback.cs   (Customer feedback)
    /// </summary>
    public partial class DatabaseService
    {
        private readonly string _connectionString = "Server=REVISION-PC\\SQLEXPRESS;Database=AviahCollectionDB;Trusted_Connection=True;Encrypt=False;";

        public DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    try
                    {
                        connection.Open();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                    }
                }
            }
            return dataTable;
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            return await Task.Run(() => ExecuteQuery(query, parameters));
        }

        public int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    try
                    {
                        connection.Open();
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                        return -1;
                    }
                }
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            return await Task.Run(() => ExecuteNonQuery(query, parameters));
        }
    }
}
