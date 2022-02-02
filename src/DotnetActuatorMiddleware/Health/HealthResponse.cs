using Newtonsoft.Json;

namespace DotnetActuatorMiddleware.Health;

public class HealthResponse
{
    [JsonProperty("healthy")]
    internal readonly bool IsHealthy;
    [JsonProperty("responseData", NullValueHandling = NullValueHandling.Ignore)]
    internal readonly object? Response;

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