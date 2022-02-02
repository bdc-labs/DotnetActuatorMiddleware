namespace DotnetActuatorMiddleware.Health.Checks;

public class MySqlHealthCheckResponse
{
    public string? Host;
    public int Port;
    public string? Version;
}