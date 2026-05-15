namespace IT13_AviahC.Services;

public static class UserSession
{
    public static int UserId { get; set; }
    public static int? CompanyId { get; set; }
    public static string? UserName { get; set; }
    public static string? UserEmail { get; set; }
    public static string? Role { get; set; }
    public static string? SelectedOrderRef { get; set; }
    public static string? CurrentTier { get; set; } // "Basic", "Standard", "Premium"
}

