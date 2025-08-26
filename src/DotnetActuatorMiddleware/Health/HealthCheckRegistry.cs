using System.Collections.Concurrent;

namespace DotnetActuatorMiddleware.Health;

public static class HealthCheckRegistry
{
    private static readonly ConcurrentDictionary<string, HealthCheck> RegisteredChecks = new ConcurrentDictionary<string, HealthCheck>();

    public static void RegisterHealthCheck(string name, Func<HealthResponse> check)
    {
        RegisteredChecks.TryAdd(name, new HealthCheck(check));
    }
    
    internal static ApplicationHealthStatus RunHealthChecks()
    {
        // Execute all registered health checks and return a Dictionary with the check name as the key and the result as the value
        var results = RegisteredChecks.ToDictionary(k => k.Key, 
            v => v.Value.Execute());

        return new ApplicationHealthStatus(results);
    }

    public static void UnregisterAllHealthChecks()
    {
        RegisteredChecks.Clear();
    }
}