using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Customer feedback submission.
    /// Used by: UserFeedbackPage
    /// </summary>
    public partial class DatabaseService
    {
        public async Task<int> SubmitFeedbackAsync(string userEmail, string subject, string message)
        {
            string query = @"
                INSERT INTO Feedback (UserId, Comments, DateSubmitted)
                VALUES ((SELECT TOP 1 Id FROM Users WHERE Email = @Email), @Comments, GETDATE())";
            
            var parameters = new Dictionary<string, object>
            {
                { "@Email", userEmail },
                { "@Comments", $"{subject}: {message}" }
            };
            
            return await ExecuteNonQueryAsync(query, parameters);
        }
    }
}
