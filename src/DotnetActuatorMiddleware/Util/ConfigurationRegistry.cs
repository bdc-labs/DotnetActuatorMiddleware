using Microsoft.Extensions.Configuration;

namespace DotnetActuatorMiddleware.Util;

/// <summary>
///  Provides static helpers for managing Microsoft configuration objects and makes them accessible anywhere in the application.
///
/// This class also allows application configuration to be exposed via the environment endpoint.
/// </summary>
public class ConfigurationRegistry
{
    internal static Dictionary<string, IConfigurationRoot> Sources { get; } = new Dictionary<string, IConfigurationRoot>();
    
    /// <summary>
    ///  Add a configuration instance to the list of available sources
    ///  <param name="configuration"><see cref="IConfiguration"/> instance to add to sources list</param>
    ///  <param name="sourceName">Name that is used to refer to this configuration source when retrieving configuration values from it</param>
    /// </summary>
    public static void AddConfigurationSource(IConfigurationRoot configuration, string sourceName)
    {
        if (Sources.ContainsKey(sourceName))
        {
            throw new InvalidOperationException($"There is already a configuration source registered with the name {sourceName}");
        }

        Sources.Add(sourceName, configuration);
    }
    
    /// <summary>
    ///  Remove the specified configuration source from the list of available sources
    ///  <param name="sourceName">Name of the source to remove</param>
    /// </summary>
    public static void RemoveConfigurationSource(string sourceName)
    {
        if (!Sources.ContainsKey(sourceName))
        {
            throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
        }

        Sources.Remove(sourceName);
    }
    
    /// <summary>
    ///  Remove all configuration sources
    /// </summary>
    public static void RemoveAllConfigurationSources()
    {
        Sources.Clear();
    }

    /// <summary>
    /// Get a configuration section from the specific source. This is useful if you need the actual <see cref="IConfigurationSection"/>
    /// object in order to easily access its children.
    /// </summary>
    /// <param name="sourceName">Configuration source to get the section from</param>
    /// <param name="key">Key of the configuration section to retrieve</param>
    /// <returns>An <see cref="IConfigurationSection"/> instance</returns>
    public static IConfigurationSection GetConfigurationSection(string sourceName, string key)
    {
        if (!Sources.ContainsKey(sourceName))
        {
            throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
        }

        return Sources[sourceName].GetSection(key);
    }
    
    /// <summary>
    ///  Gets the value of the specified key in the configuration source
    ///  <param name="sourceName">Name of configuration source to retrieve the key from</param>
    ///  <param name="key">Name of the key</param>
    /// </summary>
    public static string GetKey(string sourceName, string key)
    {
        if (!Sources.ContainsKey(sourceName))
        {
            throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
        }
        
        return Sources[sourceName].GetSection(key).Value;
    }

    /// <summary>
    ///  Sets the value of the specified key in the configuration source
    ///  <param name="sourceName">Name of configuration source where the key is to be set</param>
    ///  <param name="key">Name of the key</param>
    ///  <param name="value">Value to assign to the key</param>
    /// </summary>
    public static void SetKey(string sourceName, string key, string value)
    {
        if (!Sources.ContainsKey(sourceName))
        {
            throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
        }
        
        Sources[sourceName].GetSection(key).Value = value;
    }
    
    /// <summary>
    ///  Get all values from the specified configuration source
    ///  <param name="sourceName">Name of source to retrieve data from</param>
    ///  <returns>A dictionary containing the configuration key-value pairs</returns>
    /// </summary>
    public static Dictionary<string, string> GetAllValuesFromSource(string sourceName)
    {
        if (!Sources.ContainsKey(sourceName))
        {
            throw new InvalidOperationException("No configuration source registered with name "+ sourceName);
        }
            
        // Get children of this source and return them
        IConfiguration sourceConfiguration = Sources[sourceName];
        Dictionary<string, string> valuesDictionary = RecurseConfig(sourceConfiguration);

        //Dictionary<string, string> valuesDictionary = sourceConfiguration.GetChildren().ToDictionary(child => child.Key, child => child.Value);
        return valuesDictionary;
    }
    
    private static Dictionary<string, string> RecurseConfig(IConfiguration source)
    {
        var result = new Dictionary<string, string>();

        foreach (var child in source.GetChildren())
        {
            if (child.GetChildren().Count() != 0)
            {
                result = result.Concat(RecurseConfig(child)).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            }

            if (child.GetChildren().Count() != 0 && string.IsNullOrEmpty(child.Value))
            {
                continue;
            }

            result.Add(child.Path, child.Value);

        }

        return result;
    }
}