using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotnetActuatorMiddleware.Endpoints;
using DotnetActuatorMiddleware.Util;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Unit;

[TestFixture]
public class ApplicationEnvironmentTests
{
    [SetUp]
    public void Setup()
    {
        ConfigurationRegistry.Sources.Clear();
    }
    
    [Test]
    public void EnvironmentNoAppConfigurationTest()
    {
        var appEnvDetails = new EnvironmentEndpoint().GetEnvironment();

        Assert.That(Process.GetCurrentProcess().StartTime, Is.EqualTo(appEnvDetails.ProcessStartTime));
        Assert.That(appEnvDetails.ProcessUptimeSecs, Is.Positive);
        
        Assert.That(Environment.Version.ToString(), Is.EqualTo(appEnvDetails.FrameworkVersion));
        Assert.That(String.IsNullOrWhiteSpace(appEnvDetails.OsVersion), Is.False);
        Assert.That(appEnvDetails.Os, Is.Not.EqualTo("Unknown"));
        
        Assert.That(appEnvDetails.EnvironmentVariables.Count, Is.Not.Zero);
        
        Assert.That(appEnvDetails.ApplicationConfiguration.Count, Is.Zero);
    }

    [Test]
    public void EnvironmentWithAppConfigurationTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        ConfigurationRegistry.SetKey("memorySource", "testKey", "testValue");

        var appEnvDetails = new EnvironmentEndpoint().GetEnvironment();
        
        Assert.That(Process.GetCurrentProcess().StartTime, Is.EqualTo(appEnvDetails.ProcessStartTime));
        Assert.That(appEnvDetails.ProcessUptimeSecs, Is.Positive);
        
        Assert.That(Environment.Version.ToString(), Is.EqualTo(appEnvDetails.FrameworkVersion));
        Assert.That(String.IsNullOrWhiteSpace(appEnvDetails.OsVersion), Is.False);
        Assert.That(appEnvDetails.Os, Is.Not.EqualTo("Unknown"));
        
        Assert.That(appEnvDetails.EnvironmentVariables.Count, Is.Not.Zero);
        
        Assert.That(appEnvDetails.ApplicationConfiguration.Count, Is.Not.Zero);
        Assert.That(appEnvDetails.ApplicationConfiguration.ContainsKey("memorySource"),  Is.True);
        Assert.That(appEnvDetails.ApplicationConfiguration["memorySource"], Is.Not.Null);
        Assert.That(appEnvDetails.ApplicationConfiguration["memorySource"].ContainsKey("testKey"), Is.True);
        Assert.That(appEnvDetails.ApplicationConfiguration["memorySource"]["testKey"], Is.EqualTo("testValue"));
    }
    
}