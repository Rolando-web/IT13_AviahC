using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin;

public partial class AdminSubscriptionPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly IT13_AviahC.Services.Paymongo.PaymongoService _paymongoService;

    public AdminSubscriptionPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        // Load Paymongo Secret Key from .env file for security
        string paymongoKey = EnvService.Get("PAYMONGO_SECRET_KEY", "YOUR_PAYMONGO_SECRET_KEY");
        _paymongoService = new IT13_AviahC.Services.Paymongo.PaymongoService(paymongoKey);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshTierUI();
    }

    private void RefreshTierUI()
    {
        string currentTier = UserSession.CurrentTier ?? "Basic";
        
        // Reset all cards
        BasicPlanCard.StrokeThickness = 0;
        StandardPlanCard.StrokeThickness = 0;
        PremiumPlanCard.StrokeThickness = 0;
        
        BasicIndicator.IsVisible = false;
        StandardIndicator.IsVisible = false;
        PremiumIndicator.IsVisible = false;
        
        BasicActionBtn.Text = "Included";
        BasicActionBtn.IsEnabled = false;
        BasicActionBtn.BackgroundColor = Color.FromArgb("#94A3B8");

        StandardActionBtn.Text = "Upgrade Now";
        StandardActionBtn.IsEnabled = true;
        StandardActionBtn.BackgroundColor = Color.FromArgb("#2563EB");

        PremiumActionBtn.Text = "Upgrade Now";
        PremiumActionBtn.IsEnabled = true;
        PremiumActionBtn.BackgroundColor = Color.FromArgb("#624890");

        // Highlight current and handle permanent upgrades
        if (currentTier == "Basic")
        {
            BasicPlanCard.Stroke = Color.FromArgb("#624890");
            BasicPlanCard.StrokeThickness = 2;
            BasicActionBtn.Text = "Current Plan";
            BasicIndicator.IsVisible = true;
            BasicActionBtn.BackgroundColor = Color.FromArgb("#475569");
        }
        else if (currentTier == "Standard")
        {
            StandardPlanCard.Stroke = Color.FromArgb("#2563EB");
            StandardPlanCard.StrokeThickness = 2;
            StandardActionBtn.Text = "Current Plan";
            StandardActionBtn.IsEnabled = false;
            StandardIndicator.IsVisible = true;
            StandardActionBtn.BackgroundColor = Color.FromArgb("#475569");
            
            // Basic is already owned, disable downgrade
            BasicActionBtn.Text = "Purchased";
        }
        else if (currentTier == "Premium")
        {
            PremiumPlanCard.StrokeThickness = 2;
            PremiumActionBtn.Text = "Current Plan";
            PremiumActionBtn.IsEnabled = false;
            PremiumIndicator.IsVisible = true;
            PremiumActionBtn.BackgroundColor = Color.FromArgb("#475569");

            // Standard and Basic are owned/irrelevant, disable downgrade
            StandardActionBtn.Text = "Purchased";
            StandardActionBtn.IsEnabled = false;
            StandardActionBtn.BackgroundColor = Color.FromArgb("#94A3B8");
            
            BasicActionBtn.Text = "Purchased";
        }
    }

    private async void OnTierUpgradeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string newTier)
        {
            if (newTier == UserSession.CurrentTier) return;

            decimal amount = 0;
            if (newTier == "Standard") amount = 900;
            if (newTier == "Premium") amount = 6490;

            // Downgrades are free/immediate for this prototype
            if (amount == 0)
            {
                await ProcessTierUpdate(newTier, 0);
                return;
            }

            bool confirm = await DisplayAlertAsync("Subscription Payment", 
                $"Upgrade to {newTier} for ₱{amount}?\n\nYou will be redirected to Paymongo Secure Checkout.", "Proceed to Payment", "Cancel");
            
            if (!confirm) return;

            try
            {
                // 1. Create Paymongo Checkout Session
                var session = await _paymongoService.CreateCheckoutSessionAsync(newTier, amount);
                
                if (session != null && !string.IsNullOrEmpty(session.Data.Attributes.CheckoutUrl))
                {
                    // 2. Open Paymongo Checkout Page
                    await Browser.Default.OpenAsync(session.Data.Attributes.CheckoutUrl, BrowserLaunchMode.SystemPreferred);

                    // 3. In a real app, we would wait for a webhook. 
                    // For this ERP demo, we will simulate success after the user returns.
                    bool paid = await DisplayAlertAsync("Payment Verification", "Did you complete the payment on the Paymongo page?", "Yes, I Paid", "No, Cancel");
                    
                    if (paid)
                    {
                        await ProcessTierUpdate(newTier, amount);
                    }
                }
                else
                {
                    await DisplayAlertAsync("Payment Error", "Could not initialize Paymongo checkout session. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", "Payment integration failed: " + ex.Message, "OK");
            }
        }
    }

    private async Task ProcessTierUpdate(string newTier, decimal amount)
    {
        try 
        {
            int result = await _dbService.UpdateSubscriptionTierAsync(newTier);
            
            if (result > 0)
            {
                UserSession.CurrentTier = newTier;
                
                // Record the sale for the Superadmin to see
                if (amount > 0)
                {
                    try 
                    {
                        await _dbService.ExecuteNonQueryAsync(@"
                            INSERT INTO Orders (OrderRef, UserId, ItemSummary, TotalAmount, Status, OrderDate)
                            VALUES (@Ref, @UserId, @Summary, @Amount, 'Completed', GETDATE())",
                            new Dictionary<string, object>
                            {
                                { "@Ref", "SUB-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() },
                                { "@UserId", UserSession.UserId },
                                { "@Summary", $"Subscription Upgrade: {newTier}" },
                                { "@Amount", amount }
                            });
                    }
                    catch (Exception ex) 
                    {
                        System.Diagnostics.Debug.WriteLine("Sale record error: " + ex.Message);
                    }
                }

                await DisplayAlertAsync("Payment Successful", 
                    $"Congratulations! You have successfully upgraded to the {newTier} Tier. \n\nPlease log out and log back in to refresh your administrative dashboard and unlock your new features.", 
                    "Proceed to Relogin");
                
                RefreshTierUI();
                
                // Proactively offer to logout
                bool logoutNow = await DisplayAlertAsync("Update Applied", "Would you like to log out now to apply the changes?", "Yes, Logout", "Later");
                if (logoutNow)
                {
                    await Shell.Current.GoToAsync("//Login");
                }
            }
            else
            {
                await DisplayAlertAsync("System Error", "The payment was processed, but we could not update your account tier. Please contact support.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Critical Error", "Failed to update subscription: " + ex.Message, "OK");
        }
    }
}
