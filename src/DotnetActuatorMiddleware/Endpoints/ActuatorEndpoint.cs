using System.Net;

namespace DotnetActuatorMiddleware.Endpoints;

internal class ActuatorEndpoint
{
    internal readonly bool IpAllowListEnabled;

    internal ActuatorEndpoint(bool ipAllowListEnabled)
    {
        IpAllowListEnabled = ipAllowListEnabled;
    }

    internal bool IpIsAllowed(IPAddress ipAddress)
    {
        return ActuatorConfiguration.AllowedRanges.Count == 0 || ActuatorConfiguration.AllowedRanges.Any(r => r.Contains(ipAddress));
    }
}