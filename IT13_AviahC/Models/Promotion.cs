using System;

namespace IT13_AviahC.Models
{
    public class Promotion
    {
        public int PromoID { get; set; }
        public string? PromoCode { get; set; }
        public string? PromotionName { get; set; }
        public string? DiscountValue { get; set; }
        public string? Status { get; set; }
    }
}
