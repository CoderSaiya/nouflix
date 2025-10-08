namespace NouFlix.Models.Specification;

public class AuthOptions
{
    public string FrontendBaseUrl { get; set; } = "";
    public string[] AllowedReturnUrlHosts { get; set; } = [];
    public string SuccessPath { get; set; } = "/auth/sso/success";
    public string LoginPath { get; set; } = "/auth/login";
}