using System.Net;

namespace DotnetActuatorMiddleware.Endpoints;

internal class ActuatorEndpoint
{
    internal readonly bool IpAllowListEnabled;

    internal ActuatorEndpoint(bool ipAllowListEnabled)
    {
        IpAllowListEnabled = ipAllowListEnabled;
    }

    internal static bool IpIsAllowed(IPAddress ipAddress)
    {
        return ActuatorConfiguration.AllowedRanges.Length == 0 || ActuatorConfiguration.AllowedRanges.Any(r => r.Contains(ipAddress));
    }
}