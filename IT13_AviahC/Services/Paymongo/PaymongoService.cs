using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT13_AviahC.Services.Paymongo
{
    public class PaymongoService
    {
        private const string ApiBaseUrl = "https://api.paymongo.com/v1";
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;

        public PaymongoService(string secretKey)
        {
            _secretKey = secretKey;
            _httpClient = new HttpClient();
            
            // Basic Authentication: base64(secretKey + ":")
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_secretKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
        }

        public async Task<CheckoutResponse?> CreateCheckoutSessionAsync(string planName, decimal amount, string currency = "PHP")
        {
            var url = $"{ApiBaseUrl}/checkout_sessions";

            // Convert amount to centavos (e.g. 900 -> 90000)
            long amountInCentavos = (long)(amount * 100);

            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                        billing = new { }, // Optional: Add customer info here
                        line_items = new[]
                        {
                            new
                            {
                                amount = amountInCentavos,
                                currency = currency,
                                name = $"{planName} Subscription Plan",
                                quantity = 1,
                                description = $"Upgrade to Aviah Collection {planName} Tier"
                            }
                        },
                        payment_method_types = new[] { "gcash", "paymaya" },
                        description = $"Aviah Collection Subscription: {planName}",
                        success_url = "https://aviahcollection.com/success", // Placeholder
                        cancel_url = "https://aviahcollection.com/cancel"    // Placeholder
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<CheckoutResponse>(responseJson);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Paymongo Error: {responseJson}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Paymongo Service Exception: {ex.Message}");
                return null;
            }
        }
    }

    // Models for Paymongo API
    public class CheckoutResponse
    {
        [JsonPropertyName("data")]
        public CheckoutData Data { get; set; } = new();
    }

    public class CheckoutData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("attributes")]
        public CheckoutAttributes Attributes { get; set; } = new();
    }

    public class CheckoutAttributes
    {
        [JsonPropertyName("checkout_url")]
        public string CheckoutUrl { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
