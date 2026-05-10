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
                INSERT INTO Feedback (UserEmail, Subject, Message, SubmittedAt)
                VALUES (@UserEmail, @Subject, @Message, GETDATE())";
            
            var parameters = new Dictionary<string, object>
            {
                { "@UserEmail", userEmail },
                { "@Subject", subject },
                { "@Message", message }
            };
            
            return await ExecuteNonQueryAsync(query, parameters);
        }
    }
}
