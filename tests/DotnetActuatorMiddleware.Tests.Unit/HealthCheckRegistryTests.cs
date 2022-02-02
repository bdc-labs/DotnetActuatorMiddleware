using System;
using DotnetActuatorMiddleware.Health;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Unit;

[TestFixture]
public class HealthCheckRegistryTests
{
    [SetUp]
    public void Setup()
    {
        // Make sure we always start with a clean registry
        HealthCheckRegistry.UnregisterAllHealthChecks();
    }
    
    [Test(Description = "Check returns healthy")]
    public void HealthyTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("healthy_test", () =>
        {
            return HealthResponse.Healthy();
        });

        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.True(healthCheckResults.Results["healthy_test"].IsHealthy);
    }
    
    [Test(Description = "Check returns unhealthy")]
    public void UnhealthyTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy();
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.False(healthCheckResults.Results["unhealthy_test"].IsHealthy);
    }
    
    [Test(Description = "Check returns unhealthy with exception message as string")]
    public void UnhealthyExceptionTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy(new InvalidOperationException("Test exception"));
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.False(healthCheckResults.Results["unhealthy_test"].IsHealthy);
        Assert.NotNull(healthCheckResults.Results["unhealthy_test"].Response);
        Assert.IsInstanceOf<String>(healthCheckResults.Results["unhealthy_test"].Response);
        Assert.True(healthCheckResults.Results["unhealthy_test"].Response!.ToString()!.StartsWith("EXCEPTION: "));
    }
    
    [Test(Description = "Application wide health is true if all checks pass")]
    public void ApplicationIsHealthyTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("healthy_test1", () =>
        {
            return HealthResponse.Healthy();
        });
        
        HealthCheckRegistry.RegisterHealthCheck("healthy_test2", () =>
        {
            return HealthResponse.Healthy();
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.True(healthCheckResults.IsHealthy);
    }
    
    [Test(Description = "Application wide health is false if any check fails")]
    public void ApplicationIsUnhealthyTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("healthy_test", () =>
        {
            return HealthResponse.Healthy();
        });
        
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy();
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.False(healthCheckResults.IsHealthy);
    }

    [Test(Description = "Registration with a string output")]
    public void StringOutputObjectTest()
    {
        var testVal = "STRING";
        HealthCheckRegistry.RegisterHealthCheck("test", () =>
        {
            return HealthResponse.Healthy(testVal);
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        Assert.AreEqual(healthCheckResults.Results["test"].Response, testVal);
    }
    
    [Test(Description = "Registration with an output object")]
    public void ObjectOutputObjectTest()
    {
        var testVal = new {TestField = "Test"};
        HealthCheckRegistry.RegisterHealthCheck("test", () =>
        {
            return HealthResponse.Healthy(testVal);
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        Assert.AreEqual(healthCheckResults.Results["test"].Response, testVal);
    }
    
}