using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Customer feedback submission.
    /// Used by: UserFeedbackPage
    /// </summary>
    public partial class DatabaseService
    {
        public async Task<int> SubmitFeedbackAsync(string userEmail, string orderRef, string subject, string message)
        {
            string query = @"
                INSERT INTO Feedback (UserId, OrderRef, Comments, DateSubmitted)
                VALUES ((SELECT TOP 1 Id FROM Users WHERE Email = @Email), @OrderRef, @Comments, GETDATE())";
            
            var parameters = new Dictionary<string, object>
            {
                { "@Email", userEmail },
                { "@OrderRef", orderRef },
                { "@Comments", $"{subject}: {message}" }
            };
            
            return await ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<DataTable> GetFeedbackHistoryAsync(string userEmail)
        {
            string query = @"
                SELECT * FROM Feedback 
                WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = @Email) 
                ORDER BY DateSubmitted DESC";
            
            return await ExecuteQueryAsync(query, new Dictionary<string, object> { { "@Email", userEmail } });
        }

        public async Task<bool> CheckIfFeedbackExistsAsync(string orderRef)
        {
            string query = "SELECT COUNT(*) FROM Feedback WHERE OrderRef = @OrderRef";
            DataTable dt = await ExecuteQueryAsync(query, new Dictionary<string, object> { { "@OrderRef", orderRef } });
            if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
                return true;
            return false;
        }
    }
}
