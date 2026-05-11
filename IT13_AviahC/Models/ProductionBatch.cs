using System;

namespace IT13_AviahC.Models
{
    public class ProductionBatch
    {
        public string BatchID { get; set; } = string.Empty;
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "Unknown Product";
        public int TargetQuantity { get; set; }
        public int ProducedQuantity { get; set; }
        public int Defects { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Planned";

        // UI Helpers
        public string DateText => StartDate.ToString("MMM dd, yyyy");
        public string TargetText => $"Target: {TargetQuantity} units";
        public double Progress => TargetQuantity > 0 ? (double)ProducedQuantity / TargetQuantity : 0;
        public string ProgressText => $"{Math.Round(Progress * 100)}%";
        public double ProgressWidth => Math.Min(180, Progress * 180);

        public bool IsActive => Status == "In Progress" || Status == "Planned";

        public string StatusColorDark => Status switch
        {
            "In Progress" => "#1D4ED8",
            "Quality Check" => "#D97706",
            "Completed" => "#15803D",
            _ => "#64748B"
        };

        public string StatusColorLight => Status switch
        {
            "In Progress" => "#DBEAFE",
            "Quality Check" => "#FEF3C7",
            "Completed" => "#DCFCE7",
            _ => "#F1F5F9"
        };

        public string IconData => Status switch
        {
            "In Progress" => "M12,2C6.47,2,2,6.47,2,12s4.47,10,10,10,10-4.47,10-10S17.53,2,12,2z M12,20c-4.42,0-8-3.58-8-8s3.58-8,8-8s8,3.58,8,8 S16.42,20,12,20z M12.5,7H11v6l5.25,3.15l0.75-1.23L12.5,12.5V7z",
            "Completed" => "M9,16.17L4.83,12l-1.42,1.41L9,19 21,7l-1.41-1.41z",
            _ => "M19,3H5C3.89,3,3,3.9,3,5v14c0,1.1,0.89,2,2,2h14c1.1,0,2-0.9,2-2V5C21,3.9,20.11,3,19,3z M19,19H5V5h14V19z"
        };
    }
}
