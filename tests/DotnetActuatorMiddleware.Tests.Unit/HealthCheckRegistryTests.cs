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
        
        Assert.That(healthCheckResults.Results["healthy_test"].IsHealthy,  Is.True);
    }
    
    [Test(Description = "Check returns unhealthy")]
    public void UnhealthyTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy();
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.That(healthCheckResults.Results["unhealthy_test"].IsHealthy,  Is.False);
    }
    
    [Test(Description = "Check returns unhealthy with exception message as string")]
    public void UnhealthyExceptionTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy(new InvalidOperationException("Test exception"));
        });
        
        var healthCheckResults = HealthCheckRegistry.RunHealthChecks();
        
        Assert.That(healthCheckResults.Results["unhealthy_test"].IsHealthy, Is.False);
        Assert.That(healthCheckResults.Results["unhealthy_test"].Response, Is.Not.Null);
        Assert.That(healthCheckResults.Results["unhealthy_test"].Response, Is.InstanceOf<string>());
        Assert.That(healthCheckResults.Results["unhealthy_test"].Response!.ToString()!.StartsWith("EXCEPTION: "), Is.True);
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
        
        Assert.That(healthCheckResults.IsHealthy,  Is.True);
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
        
        Assert.That(healthCheckResults.IsHealthy,  Is.False);
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
        Assert.That(healthCheckResults.Results["test"].Response, Is.EqualTo(testVal));
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
        Assert.That(healthCheckResults.Results["test"].Response, Is.EqualTo(testVal));
    }
    
}