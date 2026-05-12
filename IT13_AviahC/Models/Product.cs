namespace IT13_AviahC.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public string? Category { get; set; }
        public int StockLevel { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string? StatusText { get; set; }
        public string? StatusColor { get; set; }
        public string? StatusTextColor { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsOnPromotion { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? PromoId { get; set; }
        public string? PromoCode { get; set; }
        public string? DiscountValue { get; set; }
        public string? PromotionName { get; set; }

        public string FormattedPrice => UnitPrice.ToString("₱#,##0.00");
        public string FormattedDiscountPrice => IsOnPromotion && DiscountPrice.HasValue 
            ? DiscountPrice.Value.ToString("₱#,##0.00") 
            : "--";
            
        public string PromoStatusText => IsOnPromotion ? "On Sale" : "Regular";
        public string PromoStatusColor => IsOnPromotion ? "#FEE2E2" : "#F1F5F9";
        public string PromoStatusTextColor => IsOnPromotion ? "#EF4444" : "#64748B";

        public string DiscountedPriceText => IsOnPromotion && DiscountPrice.HasValue 
            ? $"Sale: {DiscountPrice.Value:₱#,##0.00}" 
            : FormattedPrice;
    }
}
