using Microsoft.Data.SqlClient;
using System.Data;

namespace IT13_AviahC.Services
{
    public class DatabaseService
    {
        // Connection string for SSMS: REVISION-PC\SQLEXPRESS
        // Encrypt=False is often needed for local SQLExpress instances without certificates
        private readonly string _connectionString = "Server=REVISION-PC\\SQLEXPRESS;Database=AviahCollectionDB;Trusted_Connection=True;Encrypt=False;";

        /// <summary>
        /// Executes a query and returns a DataTable. Uses parameters to prevent SQL Injection.
        /// </summary>
        public DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters safely (PDO equivalent)
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

        /// <summary>
        /// Executes a non-query command (Insert, Update, Delete). Uses parameters to prevent SQL Injection.
        /// </summary>
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

        /// <summary>
        /// Example method to validate login using parameterized query
        /// </summary>
        public DataRow? GetUserByEmailAndPassword(string email, string password)
        {
            // SECURE: Parameters @Email and @Password prevent SQL Injection
            string query = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @Password";
            
            var parameters = new Dictionary<string, object>
            {
                { "@Email", email },
                { "@Password", password }
            };

            DataTable dt = ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public DataTable GetAllCustomers()
        {
            return ExecuteQuery("SELECT * FROM Customers ORDER BY FullName");
        }

        public DataTable GetAllSuppliers()
        {
            return ExecuteQuery("SELECT * FROM Suppliers ORDER BY CompanyName");
        }

        public DataTable GetAllInventory()
        {
            return ExecuteQuery("SELECT * FROM Inventory ORDER BY ItemName");
        }

        public DataTable GetAllPromotions()
        {
            return ExecuteQuery("SELECT * FROM Promotions ORDER BY StartDate DESC");
        }

        public DataTable GetAllDeliveries()
        {
            return ExecuteQuery("SELECT * FROM Deliveries ORDER BY DeliveryID DESC");
        }

        public DataTable GetAllSales()
        {
            return ExecuteQuery("SELECT * FROM Sales ORDER BY SalesDate DESC");
        }

        public DataTable GetAllProduction()
        {
            return ExecuteQuery("SELECT * FROM Production ORDER BY StartDate DESC");
        }

        public DataTable GetAllOrders()
        {
            return ExecuteQuery("SELECT * FROM Orders ORDER BY OrderDate DESC");
        }

        public DataTable GetAllProducts()
        {
            return ExecuteQuery("SELECT * FROM Products ORDER BY ProductName");
        }
    }
}
