using System;
using System.Net;
using System.Threading.Tasks;
using DotnetActuatorMiddleware.Env;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Integration;

[TestFixture]
public class EnvironmentEndpointMiddlewareTests
{
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
                        app.UseActuatorEnvironmentEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(allowedIp);
            c.Request.Path = "/env";
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.That(allowedContext.Response.ContentType, Is.EqualTo("application/json"));
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
                        app.UseActuatorEnvironmentEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(actualIp);
            c.Request.Path = "/env";
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.That(allowedContext.Response.StatusCode, Is.EqualTo(401));
    }
    
    [Test(Description = "Return 404 if environment endpoint not registered")]
    public async Task EndpointNotRegisteredTest() 
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorHealthEndpoint();
                        app.UseActuatorInfoEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync("/env");
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test(Description = "Endpoint returns ok with environment details")]
    public async Task EnvironmentEndpointTest()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorEnvironmentEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync("/env");

        var responseJson = JsonConvert.DeserializeObject<ApplicationEnvironment>(response.Content.ReadAsStringAsync().Result);

        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        Assert.That(responseJson, Is.Not.Null);
        Assert.That(responseJson!.EnvironmentVariables.Count, Is.Not.Zero);
        Assert.That(responseJson.ProcessId, Is.EqualTo(Environment.ProcessId));
        Assert.That(responseJson.FrameworkVersion, Is.EqualTo(Environment.Version.ToString()));
    }
}