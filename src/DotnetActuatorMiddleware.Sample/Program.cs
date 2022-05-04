using DotnetActuatorMiddleware;
using DotnetActuatorMiddleware.Health;
using DotnetActuatorMiddleware.Util;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register configuration sources for static use and environment endpoint visibility
var collection = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } };
var config1 = new ConfigurationBuilder().AddInMemoryCollection(collection).Build();
var config2 = new ConfigurationBuilder().SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("config.json", true).Build();
ConfigurationRegistry.AddConfigurationSource(config1, "memorySource");
ConfigurationRegistry.AddConfigurationSource(config2, "jsonSource");

// The above configuration sources can now be referenced easily with a static helper function
Console.WriteLine("key1 key in memorySource: "+ ConfigurationRegistry.GetKey("memorySource", "key1"));
Console.WriteLine("outerkey:innerkey key in jsonSource: " + ConfigurationRegistry.GetKey("jsonSource", "outerkey:innerkey"));

// You can also get a configuration section directly, useful if you need to iterate over its children rather than retrieving a single key's value
var jsonConfigSection = ConfigurationRegistry.GetConfigurationSection("jsonSource", "outerkey");
foreach (var childItem in jsonConfigSection.GetChildren())
{
    Console.WriteLine($"Value of {childItem.Key} in jsonSource is {childItem.Value}");
}

// Runtime configuration can be updated easily as well
ConfigurationRegistry.SetKey("jsonSource", "outerkey:innerkey", "new value");
Console.WriteLine("Modified outerkey:innerkey key in jsonSource: " + ConfigurationRegistry.GetKey("jsonSource", "outerkey:innerkey"));

/*
 * Health checks are simply functions that return either healthy or unhealthy with an optional object containing more details
 */
HealthCheckRegistry.RegisterHealthCheck("MyCustomMonitor1", () => HealthResponse.Healthy());
HealthCheckRegistry.RegisterHealthCheck("MyCustomMonitor2", () => HealthResponse.Unhealthy());
HealthCheckRegistry.RegisterHealthCheck("MyCustomMonitor3", () => HealthResponse.Healthy("Test Message"));
HealthCheckRegistry.RegisterHealthCheck("MyCustomMonitor4", () => HealthResponse.Unhealthy("Test Message2"));

// Any Lambda will also work as long as it returns a HealthResponse object
HealthCheckRegistry.RegisterHealthCheck("MyCustomMonitor5", () =>
{
    var responseDetails = new { error = "Something's wrong", code = 123 };
    return HealthResponse.Unhealthy(responseDetails);
});

/*
 * Some bundled health checks are included with the library, see the wiki at https://github.com/bdc-labs/DotnetActuatorMiddleware/wiki
 * for more details.
 */

app.MapGet("/", () => "Hello World!");
app.UseActuatorHealthEndpoint();
app.UseActuatorInfoEndpoint();
app.UseActuatorEnvironmentEndpoint();

app.Run();