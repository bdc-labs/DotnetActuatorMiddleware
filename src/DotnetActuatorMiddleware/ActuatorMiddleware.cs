using DotnetActuatorMiddleware.Endpoints;
using DotnetActuatorMiddleware.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Forbidden" }));
                    return;
                }

                var applicationHealthStatus = await Task.Run(() => HealthEndpoint.GetHealth());

                if (!applicationHealthStatus.IsHealthy)
                {
                    // Return a service unavailable status code if any of the checks fail
                    context.Response.StatusCode = 503;
                }
                
                // Assemble JSON payload
                var jsonResponseRoot = new JObject();
                jsonResponseRoot.Add("healthy", applicationHealthStatus.IsHealthy);
                foreach ((string? key, HealthResponse? value) in applicationHealthStatus.Results)
                {
                    jsonResponseRoot.Add(key, JToken.FromObject(value));
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(jsonResponseRoot));
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
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Forbidden" }));
                    return;
                }

                var infoReponse = InfoEndpoint.GetInfo();

                await context.Response.WriteAsync(JsonConvert.SerializeObject(infoReponse));
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
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Forbidden" }));
                    return;
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(EnvironmentEndpoint.GetEnvironment(), new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }));

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
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Forbidden" }));
                    return;
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(quartzEndpoint.GetSchedulerStatus()));

            }
            else
            {
                await next();
            }
        });
    }
}