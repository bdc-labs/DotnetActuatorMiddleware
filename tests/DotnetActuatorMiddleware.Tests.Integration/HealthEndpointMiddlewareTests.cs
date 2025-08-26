using System;
using System.Net;
using System.Threading.Tasks;
using DotnetActuatorMiddleware.Health;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Integration;

[TestFixture]
public class HealthEndpointMiddlewareTests
{
    private const string EndpointPath = "/health";
    private const string ContentType = "application/json";

    [SetUp]
    public void Setup()
    {
        HealthCheckRegistry.UnregisterAllHealthChecks();
        ActuatorConfiguration.ClearIpAllowList();
    }
    
    [Test(Description = "Allow client if IP is in allow list")]
    public async Task ClientIpAllowedTest()
    {
        var allowedIp = "192.168.1.1";
        
        ActuatorConfiguration.SetEndpointAllowedIps(allowedIp);
        
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorHealthEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(allowedIp);
            c.Request.Path = EndpointPath;
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.That(allowedContext.Response.ContentType, Is.EqualTo(ContentType));
        Assert.That(allowedContext.Response.StatusCode, Is.EqualTo(200));
        Assert.That(allowedContext.Response.Body, Is.Not.Null);

    }
    
    [Test(Description = "Reject client IP if not in allow list")]
    public async Task IpNotInAllowListTest()
    {
        var allowedIp = "192.168.1.1";
        var actualIp = "192.168.2.2";
        
        ActuatorConfiguration.SetEndpointAllowedIps(allowedIp);
        
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorHealthEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var rejectedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(actualIp);
            c.Request.Path = EndpointPath;
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.That(rejectedContext.Response.StatusCode, Is.EqualTo(401));
    }

    [Test(Description = "Return 404 if health endpoint not registered")]
    public async Task EndpointNotRegisteredTest() 
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorInfoEndpoint();
                        app.UseActuatorEnvironmentEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync(EndpointPath);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test(Description = "Health endpoint OK response")]
    public async Task HealthyTest() 
    {
        HealthCheckRegistry.RegisterHealthCheck("healthy_test", () =>
        {
            return HealthResponse.Healthy(new { message = "OK" });
        });
        
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorHealthEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync(EndpointPath);

        var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);

        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo(ContentType));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        Assert.That(responseJson, Is.Not.Null);
        Assert.That(responseJson.ContainsKey("healthy"), Is.True);
        Assert.That(bool.Parse(responseJson["healthy"]!.ToString()), Is.True);
        Assert.That(responseJson.ContainsKey("healthy_test"), Is.True);

        var healthCheckObj = (JObject) responseJson["healthy_test"]!;
        Assert.That(healthCheckObj, Is.Not.Null);
        Assert.That(healthCheckObj!.ContainsKey("healthy"), Is.True);
        Assert.That(bool.Parse(healthCheckObj["healthy"]!.ToString()), Is.True);
        Assert.That(healthCheckObj.ContainsKey("responseData"), Is.True);
        
        var healthCheckResponseDataObj = (JObject) responseJson["healthy_test"]!["responseData"]!;
        Assert.That(healthCheckResponseDataObj, Is.Not.Null);
        Assert.That(healthCheckResponseDataObj!.ContainsKey("message"), Is.True);
        Assert.That(healthCheckResponseDataObj["message"]!.ToString(), Is.EqualTo("OK"));
    }
    
    [Test(Description = "Health endpoint unhealthy response")]
    public async Task UnhealthyTest() 
    {
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy(new { message = "FAILED" });
        });
        
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorHealthEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync(EndpointPath);

        var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);

        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo(ContentType));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        Assert.That(responseJson, Is.Not.Null);
        Assert.That(responseJson.ContainsKey("healthy"), Is.True);
        Assert.That(bool.Parse(responseJson["healthy"]!.ToString()), Is.False);
        Assert.That(responseJson.ContainsKey("unhealthy_test"), Is.True);

        var healthCheckObj = (JObject) responseJson["unhealthy_test"]!;
        Assert.That(healthCheckObj, Is.Not.Null);
        Assert.That(healthCheckObj.ContainsKey("healthy"), Is.True);
        Assert.That(bool.Parse(healthCheckObj["healthy"]!.ToString()), Is.False);
        Assert.That(healthCheckObj.ContainsKey("responseData"), Is.True);
        
        var healthCheckResponseDataObj = (JObject) responseJson["unhealthy_test"]!["responseData"]!;
        Assert.That(healthCheckResponseDataObj, Is.Not.Null);
        Assert.That(healthCheckResponseDataObj.ContainsKey("message"), Is.True);
        Assert.That(healthCheckResponseDataObj["message"]!.ToString(), Is.EqualTo("FAILED"));
    }
    
    [Test(Description = "Health endpoint single unhealthy check")]
    public async Task SingleCheckFailureTest()
    {
        HealthCheckRegistry.RegisterHealthCheck("unhealthy_test", () =>
        {
            return HealthResponse.Unhealthy();
        });
        
        HealthCheckRegistry.RegisterHealthCheck("healthy_test", () =>
        {
            return HealthResponse.Healthy();
        });
        
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorHealthEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync(EndpointPath);

        var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);

        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo(ContentType));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        
        Assert.That(responseJson, Is.Not.Null);
        Assert.That(responseJson.ContainsKey("healthy"), Is.True);
        Assert.That(bool.Parse(responseJson["healthy"]!.ToString()), Is.False);
    }
    
}