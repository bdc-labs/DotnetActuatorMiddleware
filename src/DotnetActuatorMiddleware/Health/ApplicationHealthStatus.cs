namespace DotnetActuatorMiddleware.Health;

internal struct ApplicationHealthStatus
{
    /// <summary>
    /// Whether or not all health checks have passed
    /// </summary>
    public readonly bool IsHealthy;

    /// <summary>
    /// Array containing result of each registered health check
    /// </summary>
    public readonly Dictionary<string, HealthResponse> Results;

    public ApplicationHealthStatus(Dictionary<string, HealthResponse> results)
    {
        IsHealthy = results.All(r => r.Value.IsHealthy);
        Results = results;
    }
}