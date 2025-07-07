namespace AuthService.Infrastructure.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;

        public string? Issuer { get; set; } = "AuthService";

        public string? Audience { get; set; } = "AuthServiceClients";
        public int ExpirationInMinutes { get; set; } = 180;
    }
}
    