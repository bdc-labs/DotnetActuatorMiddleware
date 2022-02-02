using System;
using System.Collections.Generic;
using DotnetActuatorMiddleware.Util;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Unit;

[TestFixture]
public class ConfigurationRegistryTests
{
    [SetUp]
    public void Setup()
    {
        ConfigurationRegistry.Sources.Clear();
    }

    [Test(Description = "Add a configuration source")]
    public void AddSourceTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.NotZero(ConfigurationRegistry.Sources.Count);
    }
    
    [Test(Description = "Remove a configuration source")]
    public void RemoveSourceTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.True(ConfigurationRegistry.Sources.ContainsKey("memorySource"));
        
        ConfigurationRegistry.RemoveConfigurationSource("memorySource");
        
        Assert.False(ConfigurationRegistry.Sources.ContainsKey("memorySource"));
    }
    
    [Test(Description = "Try to register a duplicate configuration source")]
    public void DuplicateSourceTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.True(ConfigurationRegistry.Sources.ContainsKey("memorySource"));

        Assert.Catch<InvalidOperationException>(() =>
        {
            ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        });

    }
    
    [Test(Description = "Remove all sources")]
    public void RemoveAllSourcesTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.NotZero(ConfigurationRegistry.Sources.Count);
        
        ConfigurationRegistry.RemoveAllConfigurationSources();
        
        Assert.Zero(ConfigurationRegistry.Sources.Count);
    }
    
    [Test(Description = "Set a value in a configuration source")]
    public void SetValueTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        ConfigurationRegistry.SetKey("memorySource", "testKey", "testValue");
        var testValue = ConfigurationRegistry.GetKey("memorySource", "testKey");
        
        Assert.NotNull(testValue);
        Assert.AreEqual("testValue", testValue);
    }
    
    [Test(Description = "Get a value from a configuration source")]
    public void GetValueTest()
    {
        var collection = new Dictionary<string, string>() { { "testKey", "testValue" } };
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        var testValue = ConfigurationRegistry.GetKey("memorySource", "testKey");
        
        Assert.NotNull(testValue);
        Assert.AreEqual("testValue", testValue);
    }
    
    [Test(Description = "Get non-existent key from source")]
    public void GetNonexistentValueTest()
    {
        var collection = new Dictionary<string, string>() { { "testKey", "testValue" } };
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        var testValue = ConfigurationRegistry.GetKey("memorySource", "testKey111");
        
        Assert.IsNull(testValue);
    }
    
    [Test(Description = "Try to get key from non-existent source")]
    public void GetNonexistentSourceTest()
    {
        Assert.Catch<InvalidOperationException>((() =>
        {
            ConfigurationRegistry.GetKey("memorySource", "testKey111");
        }));
    }
    
    [Test(Description = "Get all values from a configuration source")]
    public void GetAllValuesTest()
    {
        var collection = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" } };
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");

        var configValues = ConfigurationRegistry.GetAllValuesFromSource("memorySource");
        
        Assert.Contains("key1", configValues.Keys);
        Assert.Contains("key2", configValues.Keys);
        Assert.Contains("key3", configValues.Keys);
    }
    
    [Test(Description = "Multiple sources")]
    public void MultipleSourcesTest()
    {
        var collection1 = new Dictionary<string, string>() { { "key1", "value1" } };
        var collection2 = new Dictionary<string, string>() { { "key2", "value2" } };
        var collection3 = new Dictionary<string, string>() { { "key3", "value3" } };
        
        var config1 = new ConfigurationBuilder().AddInMemoryCollection(collection1).Build();
        var config2 = new ConfigurationBuilder().AddInMemoryCollection(collection2).Build();
        var config3 = new ConfigurationBuilder().AddInMemoryCollection(collection3).Build();
        
        ConfigurationRegistry.AddConfigurationSource(config1, "memorySource1");
        ConfigurationRegistry.AddConfigurationSource(config2, "memorySource2");
        ConfigurationRegistry.AddConfigurationSource(config3, "memorySource3");
        
        var configValue1 = ConfigurationRegistry.GetKey("memorySource1", "key1");
        var configValue2 = ConfigurationRegistry.GetKey("memorySource2", "key2");
        var configValue3 = ConfigurationRegistry.GetKey("memorySource3", "key3");
        
        Assert.NotNull(configValue1);
        Assert.AreEqual("value1", configValue1);
        Assert.NotNull(configValue2);
        Assert.AreEqual("value2", configValue2);
        Assert.NotNull(configValue3);
        Assert.AreEqual("value3", configValue3);
    }
    
    
}