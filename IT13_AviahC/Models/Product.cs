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
        public int? PromoId { get; set; }
        public string? PromoCode { get; set; }
        public string? DiscountValue { get; set; }
        public string? PromotionName { get; set; }

        public string FormattedPrice => UnitPrice.ToString("₱#,##0.00");
        public string DiscountedPriceText => string.IsNullOrEmpty(DiscountValue) ? FormattedPrice : $"Discounted: {CalculateDiscountedPrice():₱#,##0.00}";

        private decimal CalculateDiscountedPrice()
        {
            if (string.IsNullOrEmpty(DiscountValue)) return UnitPrice;
            
            string val = DiscountValue.Replace("%", "").Replace("OFF", "").Trim();
            if (decimal.TryParse(val, out decimal discount))
            {
                if (DiscountValue.Contains("%"))
                    return UnitPrice * (1 - (discount / 100));
                else
                    return Math.Max(0, UnitPrice - discount);
            }
            return UnitPrice;
        }
    }
}
