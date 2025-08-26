using System.Reflection;

namespace DotnetActuatorMiddleware.Endpoints;

internal class InfoEndpoint : ActuatorEndpoint
{
    internal InfoEndpoint(bool ipAllowListEnabled = false) : base(ipAllowListEnabled) { }

    internal static InfoEndpointResponse GetInfo()
    {
        AssemblyName? assemblyInfo = Assembly.GetEntryAssembly()?.GetName();

        if (assemblyInfo is null)
        {
            return new InfoEndpointResponse { Name = "NotAvailable", Version = "0.0.0.0" };
        }

        return new InfoEndpointResponse
        {
            Name = assemblyInfo.Name, 
            Version = assemblyInfo.Version?.ToString()
        };
    }
}