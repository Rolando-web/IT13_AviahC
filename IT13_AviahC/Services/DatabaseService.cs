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

        public string GetActiveSubscriptionTier()
        {
            // Default to Premium if table doesn't exist or no tier is set
            try
            {
                DataTable dt = ExecuteQuery("SELECT TOP 1 TierName FROM SubscriptionSettings");
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["TierName"]?.ToString() ?? "Basic";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching subscription tier: {ex.Message}");
            }
            return "Basic";
        }

        public async Task<int> UpdateSubscriptionTierAsync(string tierName)
        {
            System.Diagnostics.Debug.WriteLine($"Attempting to update SubscriptionTier to {tierName} for UserId: {UserSession.UserId}");
            
            // 1. Update Specific User Account (Primary)
            string userQuery = "UPDATE Users SET SubscriptionTier = @Tier WHERE Id = @UserId";
            var userParams = new Dictionary<string, object>
            {
                { "@Tier", tierName },
                { "@UserId", UserSession.UserId }
            };
            int result = await ExecuteNonQueryAsync(userQuery, userParams);

            // 2. Update Global Settings (Secondary)
            try 
            {
                string globalQuery = "IF EXISTS (SELECT 1 FROM SubscriptionSettings) UPDATE SubscriptionSettings SET TierName = @Tier, LastUpdated = GETDATE(), UpdatedBy = @User ELSE INSERT INTO SubscriptionSettings (TierName, LastUpdated, UpdatedBy) VALUES (@Tier, GETDATE(), @User)";
                var globalParams = new Dictionary<string, object>
                {
                    { "@Tier", tierName },
                    { "@User", UserSession.UserName ?? "Admin" }
                };
                await ExecuteNonQueryAsync(globalQuery, globalParams);
            }
            catch { /* Ignore if table doesn't exist yet */ }

            return result;
        }
    }
}
