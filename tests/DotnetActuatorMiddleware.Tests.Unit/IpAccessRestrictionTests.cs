using System;
using System.Net;
using DotnetActuatorMiddleware.Endpoints;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Unit;

[TestFixture]
public class IpAccessRestrictionTests
{
    [SetUp]
    public void Setup()
    {
        ActuatorConfiguration.ClearIpAllowList();
    }
    
    [Test(Description = "Comma-separated list of allowed IPs")]
    public void MultipleIpStringTest()
    {
        var allowedIpString = "192.168.0.0/16,10.0.0.0/8";
        
        ActuatorConfiguration.SetEndpointAllowedIps(allowedIpString);

        var actuatorEndpoint = new ActuatorEndpoint(true);

        Assert.That(ActuatorEndpoint.IpIsAllowed(IPAddress.Parse("192.168.1.1")),  Is.True);
        Assert.That(ActuatorEndpoint.IpIsAllowed(IPAddress.Parse("10.255.255.1")), Is.True);
        Assert.That(ActuatorEndpoint.IpIsAllowed(IPAddress.Parse("172.21.1.1")), Is.False);
        
    }
    
    [Test(Description = "Allow single IP")]
    public void AllowSingleIpTest()
    {
        var allowedIpString = "192.168.1.1";
        
        ActuatorConfiguration.SetEndpointAllowedIps(allowedIpString);

        var actuatorEndpoint = new ActuatorEndpoint(true);
        
        Assert.That(ActuatorEndpoint.IpIsAllowed(IPAddress.Parse("192.168.1.1")),  Is.True);
        Assert.That(ActuatorEndpoint.IpIsAllowed(IPAddress.Parse("192.168.1.2")), Is.False);

    }
    
    [Test(Description = "Invalid IP string throws exception")]
    public void InvalidIpStringTest()
    {
        Assert.Catch<InvalidOperationException>((() =>
        {
            ActuatorConfiguration.SetEndpointAllowedIps("1.1.1.1.1");
        }));
    }
}