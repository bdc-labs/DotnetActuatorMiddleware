using System;
using System.Collections.Generic;
using System.Linq;

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
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.That(ConfigurationRegistry.Sources.Count, Is.Not.Zero);
    }
    
    [Test(Description = "Remove a configuration source")]
    public void RemoveSourceTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.That(ConfigurationRegistry.Sources.ContainsKey("memorySource"), Is.True);
        
        ConfigurationRegistry.RemoveConfigurationSource("memorySource");
        
        Assert.That(ConfigurationRegistry.Sources.ContainsKey("memorySource"), Is.False);
    }
    
    [Test(Description = "Try to register a duplicate configuration source")]
    public void DuplicateSourceTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.That(ConfigurationRegistry.Sources.ContainsKey("memorySource"), Is.True);

        Assert.Catch<InvalidOperationException>(() =>
        {
            ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        });

    }
    
    [Test(Description = "Remove all sources")]
    public void RemoveAllSourcesTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        Assert.That(ConfigurationRegistry.Sources.Count, Is.Not.Zero);
        
        ConfigurationRegistry.RemoveAllConfigurationSources();
        
        Assert.That(ConfigurationRegistry.Sources.Count, Is.Zero);
    }
    
    [Test(Description = "Set a value in a configuration source")]
    public void SetValueTest()
    {
        var collection = new Dictionary<string, string>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        ConfigurationRegistry.SetKey("memorySource", "testKey", "testValue");
        var testValue = ConfigurationRegistry.GetKey("memorySource", "testKey");
        
        Assert.That(testValue, Is.Not.Null);
        Assert.That(testValue, Is.EqualTo("testValue"));
    }
    
    [Test(Description = "Get a value from a configuration source")]
    public void GetValueTest()
    {
        var collection = new Dictionary<string, string>() { { "testKey", "testValue" } };
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        var testValue = ConfigurationRegistry.GetKey("memorySource", "testKey");
        
        Assert.That(testValue, Is.Not.Null);
        Assert.That(testValue, Is.EqualTo("testValue"));
    }
    
    [Test(Description = "Get a configuration section from a configuration source")]
    public void GetConfigurationSectionTest()
    {
        var collection = new Dictionary<string, string>() { { "outerkey:innerkey", "testValue" } };
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        var testSection = ConfigurationRegistry.GetConfigurationSection("memorySource", "outerkey");
        var testSectionChildKey = testSection.GetChildren().ToList()[0];
        
        Assert.That(testSectionChildKey,  Is.Not.Null);
        Assert.That(testSectionChildKey.Key, Is.EqualTo("innerkey"));
        Assert.That(testSectionChildKey.Value, Is.EqualTo("testValue"));
    }
    
    [Test(Description = "Get non-existent key from source")]
    public void GetNonexistentValueTest()
    {
        var collection = new Dictionary<string, string>() { { "testKey", "testValue" } };
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");
        
        var testValue = ConfigurationRegistry.GetKey("memorySource", "testKey111");
        
        Assert.That(testValue, Is.Null);
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
        var config = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        ConfigurationRegistry.AddConfigurationSource(config, "memorySource");

        var configValues = ConfigurationRegistry.GetAllValuesFromSource("memorySource");
        
        Assert.That(configValues.Keys, Contains.Item("key1"));
        Assert.That(configValues.Keys, Contains.Item("key2"));
        Assert.That(configValues.Keys, Contains.Item("key3"));
    }
    
    [Test(Description = "Multiple sources")]
    public void MultipleSourcesTest()
    {
        var collection1 = new Dictionary<string, string>() { { "key1", "value1" } };
        var collection2 = new Dictionary<string, string>() { { "key2", "value2" } };
        var collection3 = new Dictionary<string, string>() { { "key3", "value3" } };
        
        var config1 = new ConfigurationBuilder().AddInMemoryCollection(collection1!).Build();
        var config2 = new ConfigurationBuilder().AddInMemoryCollection(collection2!).Build();
        var config3 = new ConfigurationBuilder().AddInMemoryCollection(collection3!).Build();
        
        ConfigurationRegistry.AddConfigurationSource(config1, "memorySource1");
        ConfigurationRegistry.AddConfigurationSource(config2, "memorySource2");
        ConfigurationRegistry.AddConfigurationSource(config3, "memorySource3");
        
        var configValue1 = ConfigurationRegistry.GetKey("memorySource1", "key1");
        var configValue2 = ConfigurationRegistry.GetKey("memorySource2", "key2");
        var configValue3 = ConfigurationRegistry.GetKey("memorySource3", "key3");
        
        Assert.That(configValue1, Is.Not.Null);
        Assert.That(configValue1, Is.EqualTo("value1"));
        Assert.That(configValue2, Is.Not.Null);
        Assert.That(configValue2, Is.EqualTo("value2"));
        Assert.That(configValue3, Is.Not.Null);
        Assert.That(configValue3, Is.EqualTo("value3"));
    }
    
    
}