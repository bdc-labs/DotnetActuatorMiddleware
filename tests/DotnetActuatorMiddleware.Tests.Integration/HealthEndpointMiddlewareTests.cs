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
        
        Assert.AreEqual(ContentType, allowedContext.Response.ContentType);
        Assert.AreEqual(200, allowedContext.Response.StatusCode);
        Assert.NotNull(allowedContext.Response.Body);

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

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(actualIp);
            c.Request.Path = EndpointPath;
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.AreEqual(401, allowedContext.Response.StatusCode);
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
        
        Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);
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

        Assert.AreEqual(ContentType, response.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        
        Assert.NotNull(responseJson);
        Assert.True(responseJson.ContainsKey("healthy"));
        Assert.AreEqual(true, bool.Parse(responseJson["healthy"]!.ToString()));
        Assert.True(responseJson.ContainsKey("healthy_test"));

        var healthCheckObj = (JObject) responseJson["healthy_test"]!;
        Assert.NotNull(healthCheckObj);
        Assert.True(healthCheckObj!.ContainsKey("healthy"));
        Assert.AreEqual(true, bool.Parse(healthCheckObj["healthy"]!.ToString()));
        Assert.True(healthCheckObj.ContainsKey("responseData"));
        
        var healthCheckResponseDataObj = (JObject) responseJson["healthy_test"]!["responseData"]!;
        Assert.NotNull(healthCheckResponseDataObj);
        Assert.True(healthCheckResponseDataObj!.ContainsKey("message"));
        Assert.AreEqual("OK", healthCheckResponseDataObj["message"]!.ToString());
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

        Assert.AreEqual(ContentType, response.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(response.StatusCode, HttpStatusCode.ServiceUnavailable);
        
        Assert.NotNull(responseJson);
        Assert.True(responseJson.ContainsKey("healthy"));
        Assert.AreEqual(false, bool.Parse(responseJson["healthy"]!.ToString()));
        Assert.True(responseJson.ContainsKey("unhealthy_test"));

        var healthCheckObj = (JObject) responseJson["unhealthy_test"]!;
        Assert.NotNull(healthCheckObj);
        Assert.True(healthCheckObj.ContainsKey("healthy"));
        Assert.AreEqual(false, bool.Parse(healthCheckObj["healthy"]!.ToString()));
        Assert.True(healthCheckObj.ContainsKey("responseData"));
        
        var healthCheckResponseDataObj = (JObject) responseJson["unhealthy_test"]!["responseData"]!;
        Assert.NotNull(healthCheckResponseDataObj);
        Assert.True(healthCheckResponseDataObj.ContainsKey("message"));
        Assert.AreEqual("FAILED", healthCheckResponseDataObj["message"]!.ToString());
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

        Assert.AreEqual(ContentType, response.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(response.StatusCode, HttpStatusCode.ServiceUnavailable);
        
        Assert.NotNull(responseJson);
        Assert.True(responseJson.ContainsKey("healthy"));
        Assert.AreEqual(false, bool.Parse(responseJson["healthy"]!.ToString()));
    }
    
}