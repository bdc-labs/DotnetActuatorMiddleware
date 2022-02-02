using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Integration;

[TestFixture]
public class InfoEndpointMiddlewareTests
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
                        app.UseActuatorInfoEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(allowedIp);
            c.Request.Path = "/info";
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
                        app.UseActuatorInfoEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(actualIp);
            c.Request.Path = "/info";
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.AreEqual(401, allowedContext.Response.StatusCode);
    }

    [Test(Description = "Return 404 if info endpoint not registered")]
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
                        app.UseActuatorEnvironmentEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync("/info");
        
        Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);
    }
    
    [Test(Description = "Endpoint returns ok with assembly info")]
    public async Task InfoEndpointTest()
    {
        var assemblyDetails = Assembly.GetEntryAssembly()!.GetName();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorInfoEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync("/info");

        var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
        
        Assert.AreEqual("application/json", response.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        
        Assert.True(responseJson.ContainsKey("Name"));
        Assert.True(responseJson.ContainsKey("Version"));
        
        Assert.AreEqual(assemblyDetails.Name, responseJson["Name"]!.ToString());
        Assert.AreEqual(assemblyDetails.Version!.ToString(), responseJson["Version"]!.ToString());
    }
}