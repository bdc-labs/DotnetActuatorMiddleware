using DotnetActuatorMiddleware.Health;

namespace DotnetActuatorMiddleware.Endpoints;

internal class HealthEndpoint : ActuatorEndpoint
{
    internal HealthEndpoint(bool ipAllowListEnabled = false) : base(ipAllowListEnabled) { }

    internal ApplicationHealthStatus GetHealth()
    {
        return HealthCheckRegistry.RunHealthChecks();
    }
}