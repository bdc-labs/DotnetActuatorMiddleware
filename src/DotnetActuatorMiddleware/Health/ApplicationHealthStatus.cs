using System.Text.Json.Serialization;

namespace DotnetActuatorMiddleware.Health;

internal struct ApplicationHealthStatus
{
    /// <summary>
    /// Whether all health checks have passed
    /// </summary>
    [JsonPropertyName("healthy")]
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Array containing result of each registered health check
    /// </summary>
    public Dictionary<string, HealthResponse> Results { get; set; }

    public ApplicationHealthStatus(Dictionary<string, HealthResponse> results)
    {
        IsHealthy = results.All(r => r.Value.IsHealthy);
        Results = results;
    }
}