using System.Text.Json.Serialization;

namespace DotnetActuatorMiddleware.Health;

public class HealthResponse
{
    [JsonPropertyName("healthy")]
    public bool IsHealthy { get; set; }
    [JsonPropertyName("responseData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Response { get; set; }

    internal HealthResponse(bool isHealthy, object statusObject)
    {
        IsHealthy = isHealthy;
        Response = statusObject;
    }
    
    internal HealthResponse(bool isHealthy)
    {
        IsHealthy = isHealthy;
    }

    public static HealthResponse Healthy()
    {
        return new HealthResponse(true);
    }
    
    public static HealthResponse Healthy(object response)
    {
        return new HealthResponse(true, response);
    }

    public static HealthResponse Unhealthy()
    {
        return new HealthResponse(false);
    }

    public static HealthResponse Unhealthy(object response)
    {
        return new HealthResponse(false, response);
    }

    public static HealthResponse Unhealthy(Exception exception)
    {
        var message = $"EXCEPTION: {exception.GetType().Name}, {exception.Message}";
        return new HealthResponse(false, message);
    }
}