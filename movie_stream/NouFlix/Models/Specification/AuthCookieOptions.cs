namespace NouFlix.Models.Specification;

public class AuthCookieOptions
{
    // Tên cookie refresh
    public string RefreshCookieName { get; set; } = "refresh_token";
    // Path áp dụng cookie (mặc định toàn site)
    public string Path { get; set; } = "/";
    // Domain (để trống nếu cùng domain với API). Ví dụ: ".example.com"
    public string? Domain { get; set; }
    // TTL mặc định nếu không truyền expiresAt khi ghi
    public int RefreshDays { get; set; } = 7;
    // Nếu FE và API khác domain (cross-site) → cần SameSite=None + Secure
    public bool CrossSite { get; set; } = true;
}