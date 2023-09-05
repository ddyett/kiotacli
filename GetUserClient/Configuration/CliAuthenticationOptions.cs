using GetUserClient.Authentication;

namespace GetUserClient.Configuration;

public class CliAuthenticationOptions
{
    public string? Authority { get; set; }

    public string? TenantId { get; set; }

    public string? ClientId { get; set; }

    public string? ClientCertificateName { get; set; }

    public string? ClientCertificateThumbPrint { get; set; }
    
    public AuthenticationStrategy Strategy { get; set; } = AuthenticationStrategy.DeviceCode;
}