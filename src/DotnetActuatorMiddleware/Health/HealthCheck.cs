namespace DotnetActuatorMiddleware.Health;

public class HealthCheck
{
    private readonly Func<HealthResponse> _checkFunc;

    internal HealthCheck(Func<HealthResponse> check)
    {
        _checkFunc = check;
    }

    internal HealthResponse Execute()
    {
        try
        {
            return _checkFunc();
        }
        catch (Exception x)
        {
            return HealthResponse.Unhealthy(x);
        }
    }

}