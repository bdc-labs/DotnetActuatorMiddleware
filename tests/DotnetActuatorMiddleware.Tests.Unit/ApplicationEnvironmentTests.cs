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

        Assert.AreEqual(Process.GetCurrentProcess().StartTime, appEnvDetails.ProcessStartTime);
        Assert.Positive(appEnvDetails.ProcessUptimeSecs);
        
        Assert.AreEqual(Environment.Version.ToString(), appEnvDetails.FrameworkVersion);
        Assert.False(String.IsNullOrWhiteSpace(appEnvDetails.OsVersion));
        Assert.AreNotEqual("Unknown", appEnvDetails.Os);
        
        Assert.NotZero(appEnvDetails.EnvironmentVariables.Count);
        
        Assert.Zero(appEnvDetails.ApplicationConfiguration.Count);
    }

    [Test]
    public void EnvironmentWithAppConfigurationTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        ConfigurationRegistry.SetKey("memorySource", "testKey", "testValue");

        var appEnvDetails = new EnvironmentEndpoint().GetEnvironment();
        
        Assert.AreEqual(Process.GetCurrentProcess().StartTime, appEnvDetails.ProcessStartTime);
        Assert.Positive(appEnvDetails.ProcessUptimeSecs);
        
        Assert.AreEqual(Environment.Version.ToString(), appEnvDetails.FrameworkVersion);
        Assert.False(String.IsNullOrWhiteSpace(appEnvDetails.OsVersion));
        Assert.AreNotEqual("Unknown", appEnvDetails.Os);
        
        Assert.NotZero(appEnvDetails.EnvironmentVariables.Count);
        
        Assert.NotZero(appEnvDetails.ApplicationConfiguration.Count);
        Assert.True(appEnvDetails.ApplicationConfiguration.ContainsKey("memorySource"));
        Assert.NotNull(appEnvDetails.ApplicationConfiguration["memorySource"]);
        Assert.True(appEnvDetails.ApplicationConfiguration["memorySource"].ContainsKey("testKey"));
        Assert.AreEqual("testValue", appEnvDetails.ApplicationConfiguration["memorySource"]["testKey"]);
    }
    
}