namespace GetUserClient.Authentication;

public enum AuthenticationStrategy
{
    DeviceCode,
    InteractiveBrowser,
    ClientCertificate,
    Environment
}

public static class AuthenticationStrategyExtensions
{
    public static bool IsPrivateClient(this AuthenticationStrategy strategy)
    {
        return strategy switch
        {
            AuthenticationStrategy.DeviceCode or AuthenticationStrategy.InteractiveBrowser => false,
            AuthenticationStrategy.ClientCertificate or AuthenticationStrategy.Environment => true,
            _ => throw new System.ArgumentOutOfRangeException(nameof(strategy), strategy, "The authentication strategy is invalid")
        };
    }
}