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
        
        Assert.AreEqual("application/json", allowedContext.Response.ContentType);
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
        
        Assert.AreEqual(401, allowedContext.Response.StatusCode);
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
        
        Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);
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

        Assert.AreEqual("application/json", response.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        
        Assert.NotNull(responseJson);
        Assert.NotZero(responseJson!.EnvironmentVariables.Count);
        Assert.AreEqual(Environment.ProcessId, responseJson.ProcessId);
        Assert.AreEqual(Environment.Version.ToString(), responseJson.FrameworkVersion);
    }
}