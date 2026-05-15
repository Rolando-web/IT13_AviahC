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

        public void RecordLoginSession(int userId, string deviceType = "Desktop")
        {
            string query = "INSERT INTO UserSessions (UserId, DeviceType) VALUES (@UserId, @DeviceType)";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@DeviceType", deviceType }
            };
            ExecuteNonQuery(query, parameters);
        }
    }
}
