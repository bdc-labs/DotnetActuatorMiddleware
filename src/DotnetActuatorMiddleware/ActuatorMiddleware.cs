using System.Text.Json;

using DotnetActuatorMiddleware.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DotnetActuatorMiddleware;

public static class ActuatorMiddleware
{
    public static void UseActuatorHealthEndpoint(this IApplicationBuilder app, bool ipAllowListEnabled = false)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value is not null && context.Request.Path.Value == "/health")
            {
                context.Response.Headers.ContentType = "application/json";

                var healthEndpoint = new HealthEndpoint(ipAllowListEnabled);
                
                // Check if request is coming from an allowed IP
                if (healthEndpoint.IpAllowListEnabled && context.Connection.RemoteIpAddress is not null && !ActuatorEndpoint.IpIsAllowed(context.Connection.RemoteIpAddress))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Forbidden" }));
                    return;
                }

                var applicationHealthStatus = await Task.Run(() => HealthEndpoint.GetHealth());

                if (!applicationHealthStatus.IsHealthy)
                {
                    // Return a service unavailable status code if any of the checks fail
                    context.Response.StatusCode = 503;
                }
                
                // System.text.json doesn't appear to flatten the same way that Newtonsoft did so we do that here
                // while trying to be somewhat memory efficient about it
                await using var json = new Utf8JsonWriter(
                    context.Response.BodyWriter,
                    new JsonWriterOptions { SkipValidation = true }
                );
                
                json.WriteStartObject();
                json.WriteBoolean("healthy", applicationHealthStatus.IsHealthy);
                
                foreach (var kv in applicationHealthStatus.Results)
                {
                    json.WritePropertyName(kv.Key);
                    JsonSerializer.Serialize(json, kv.Value);
                }

                json.WriteEndObject();
                await json.FlushAsync();
            }
            else
            {
                await next();
            }
        });
    }
    
    public static void UseActuatorInfoEndpoint(this IApplicationBuilder app, bool ipAllowListEnabled = false)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value is not null && context.Request.Path.Value == "/info")
            {
                context.Response.Headers.ContentType = "application/json";

                var infoEndpoint = new InfoEndpoint(ipAllowListEnabled);
                
                // Check if request is coming from an allowed IP
                if (infoEndpoint.IpAllowListEnabled && context.Connection.RemoteIpAddress is not null && !ActuatorEndpoint.IpIsAllowed(context.Connection.RemoteIpAddress))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Forbidden" }));
                    return;
                }

                var infoReponse = InfoEndpoint.GetInfo();

                await context.Response.WriteAsync(JsonSerializer.Serialize(infoReponse));
            }
            else
            {
                await next();
            }
        });
    }

    public static void UseActuatorEnvironmentEndpoint(this IApplicationBuilder app, bool ipAllowListEnabled = false)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value is not null && context.Request.Path.Value == "/env")
            {
                context.Response.Headers.ContentType = "application/json";

                var environmentEndpoint = new EnvironmentEndpoint(ipAllowListEnabled);
                
                // Check if request is coming from an allowed IP
                if (environmentEndpoint.IpAllowListEnabled && context.Connection.RemoteIpAddress is not null && !ActuatorEndpoint.IpIsAllowed(context.Connection.RemoteIpAddress))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Forbidden" }));
                    return;
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(EnvironmentEndpoint.GetEnvironment()));

            }
            else
            {
                await next();
            }
        });
    }
    
    public static void UseActuatorQuartzEndpoint(this IApplicationBuilder app, bool ipAllowListEnabled = false)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value is not null && context.Request.Path.Value == "/quartz")
            {
                context.Response.Headers.ContentType = "application/json";

                var quartzEndpoint = new QuartzEndpoint(ipAllowListEnabled);
                
                // Check if request is coming from an allowed IP
                if (quartzEndpoint.IpAllowListEnabled && context.Connection.RemoteIpAddress is not null && !ActuatorEndpoint.IpIsAllowed(context.Connection.RemoteIpAddress))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Forbidden" }));
                    return;
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(quartzEndpoint.GetSchedulerStatus()));

            }
            else
            {
                await next();
            }
        });
    }
}