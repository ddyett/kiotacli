namespace GetUserClient.Configuration;

public class ConfigurationRoot
{
    public CliAuthenticationOptions AuthenticationOptions { get; set; } = new();
}