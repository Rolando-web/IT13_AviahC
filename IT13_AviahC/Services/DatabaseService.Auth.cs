using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Authentication and user lookup methods.
    /// Used by: LoginPage
    /// </summary>
    public partial class DatabaseService
    {
        public DataRow? GetUserByEmailAndPassword(string email, string password)
        {
            string query = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @Password";
            var parameters = new Dictionary<string, object>
            {
                { "@Email", email },
                { "@Password", password }
            };
            DataTable dt = ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
    }
}
