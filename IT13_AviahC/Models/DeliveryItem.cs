namespace IT13_AviahC.Models
{
    public class DeliveryItem
    {
        public string DeliveryID { get; set; } = string.Empty;
        public string OrderRef { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string CurrentLocation { get; set; } = "Unknown";
        public string Destination { get; set; } = "Not set";
        public string ETA { get; set; } = "TBD";
        public string DriverName { get; set; } = "Unassigned";
        public string ProductImage { get; set; } = "dress.png";

        public string StatusBgLight => Status switch
        {
            "Delivered" or "Package delivered successfully" => "#DCFCE7",
            "In Transit" or "Out for delivery" or "The order is in transit and is on its way to the next location" or 
            "Parcel has departed from sorting facility" or "Parcel has arrived at sorting facility" => "#EEF2FF",
            "Out for Delivery" or "Preparing to ship" or "Your parcel has been picked up by our logistics partner" or
            "Parcel has been received at dropoff point" or "Parcel is loaded into truck, to leave sorting center soon" => "#FEF3C7",
            "Failed" => "#FEE2E2",
            "Pending" or "Order placed" => "#F3F0FA",
            _ => "#F1F5F9"
        };

        public string StatusColorDark => Status switch
        {
            "Delivered" or "Package delivered successfully" => "#15803D",
            "In Transit" or "Out for delivery" or "The order is in transit and is on its way to the next location" or
            "Parcel has departed from sorting facility" or "Parcel has arrived at sorting facility" => "#4338CA",
            "Out for Delivery" or "Preparing to ship" or "Your parcel has been picked up by our logistics partner" or
            "Parcel has been received at dropoff point" or "Parcel is loaded into truck, to leave sorting center soon" => "#D97706",
            "Failed" => "#DC2626",
            "Pending" or "Order placed" => "#624890",
            _ => "#64748B"
        };
    }
}
