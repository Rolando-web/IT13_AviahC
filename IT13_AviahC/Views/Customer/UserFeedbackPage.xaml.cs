using Microsoft.Maui.Controls;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer
{
    public partial class UserFeedbackPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public UserFeedbackPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private async void OnSubmitFeedbackClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SubjectEntry.Text) || string.IsNullOrWhiteSpace(MessageEditor.Text))
                {
                    await DisplayAlertAsync("Validation", "Please fill in both the subject and your message.", "OK");
                    return;
                }

                // Disable button to prevent double submission
                SubmitButton.IsEnabled = false;
                SubmitButton.Text = "Submitting...";

                string userEmail = UserSession.UserEmail ?? "customer@aviah.com";
                int result = await _databaseService.SubmitFeedbackAsync(userEmail, SubjectEntry.Text, MessageEditor.Text);

                if (result > 0)
                {
                    await DisplayAlertAsync("Success", "Your feedback has been submitted successfully! Our team will review it shortly.", "OK");
                    
                    // Clear the form
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SubjectEntry.Text = string.Empty;
                        MessageEditor.Text = string.Empty;
                    });
                }
                else
                {
                    await DisplayAlertAsync("Error", "Failed to submit feedback. Please try again later.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Feedback Error: {ex.Message}");
                await DisplayAlertAsync("Error", "Something went wrong. Please try again.", "OK");
            }
            finally
            {
                // Re-enable button
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SubmitButton.IsEnabled = true;
                    SubmitButton.Text = "✉  Submit Feedback";
                });
            }
        }
    }
}
