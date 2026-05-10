namespace IT13_AviahC.Models
{
    public class OfferItem
    {
        public string ItemName { get; set; } = string.Empty;
        public string OriginalPrice { get; set; } = string.Empty;
        public string PromoPrice { get; set; } = string.Empty;
        public string DiscountPercent { get; set; } = string.Empty;
        public string Savings { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? PromotionName { get; set; }
    }

    public class OrderItem
    {
        public string OrderRef { get; set; } = string.Empty;
        public string OrderDateAndSummary { get; set; } = string.Empty;
        public string FormattedTotal { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public bool IsTrackable { get; set; }
    }
}
