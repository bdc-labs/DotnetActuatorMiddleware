using DotnetActuatorMiddleware.Env;

namespace DotnetActuatorMiddleware.Endpoints;

internal class EnvironmentEndpoint : ActuatorEndpoint
{
    internal EnvironmentEndpoint(bool ipAllowListEnabled = false) : base(ipAllowListEnabled) { }

    internal static ApplicationEnvironment GetEnvironment()
    {
        return new ApplicationEnvironment();
    }
}