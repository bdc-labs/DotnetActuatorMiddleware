using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotnetActuatorMiddleware.Endpoints;
using DotnetActuatorMiddleware.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;

namespace DotnetActuatorMiddleware.Tests.Integration;

[TestFixture]
public class QuartzEndpointTests
{
    private const string EndpointPath = "/quartz";
    private const string ContentType = "application/json";
    private readonly IScheduler _defaultScheduler = new StdSchedulerFactory().GetScheduler().Result;

    [SetUp]
    public void SetUp()
    {
        // Make sure the scheduler is clean for each test
        _defaultScheduler.Clear();
    }
    
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
                        app.UseActuatorQuartzEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(allowedIp);
            c.Request.Path = EndpointPath;
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.That(allowedContext.Response.ContentType, Is.EqualTo("application/json"));
        Assert.That(allowedContext.Response.StatusCode, Is.EqualTo(200));
        Assert.That(allowedContext.Response.Body, Is.Not.Null);

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
                        app.UseActuatorQuartzEndpoint(true);
                    });
            }).StartAsync();

        var server = host.GetTestServer();

        var allowedContext = await server.SendAsync((c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse(actualIp);
            c.Request.Path = EndpointPath;
            c.Request.Method = HttpMethods.Get;
        }));
        
        Assert.That(allowedContext.Response.StatusCode, Is.EqualTo(401));
    }

    [Test(Description = "Return 404 if quartz endpoint not registered")]
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

        var response = await host.GetTestClient().GetAsync(EndpointPath);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test(Description = "Simple scheduler with a single registered job")]
    public async Task SimpleSchedulerTest()
    {
        IJobDetail testJob = JobBuilder.Create<TestQuartzJob>()
            .WithIdentity("testJob", "group1")
            .Build();
        
        ITrigger testTrigger = TriggerBuilder.Create()
            .WithIdentity("testTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(5)
                .WithRepeatCount(1))
            .Build();
        
        await _defaultScheduler.ScheduleJob(testJob, testTrigger);
        await _defaultScheduler.Start();
        Thread.Sleep(1000);
        
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorQuartzEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync(EndpointPath);
        
        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseObj = JsonConvert.DeserializeObject<QuartzEndpointResponse>(response.Content.ReadAsStringAsync().Result);
        
        Assert.That(responseObj.Schedulers.Count, Is.EqualTo(1));
        Assert.That(responseObj.Schedulers, Contains.Key("DefaultQuartzScheduler"));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].SchedulerStatus, Is.EqualTo("STARTED"));

        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs.Count, Is.EqualTo(1));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Name, Is.EqualTo("testJob"));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Group, Is.EqualTo("group1"));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Description, Is.Null);

        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Triggers.Count, Is.Not.Zero);
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Triggers[0].Name, Is.EqualTo("testTrigger"));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Triggers[0].Group, Is.EqualTo("group1"));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Triggers[0].StartTimeUtc, Is.Not.Null);
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Triggers[0].NextFireTimeUtc, Is.Not.Null);
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs[0].Triggers[0].LastFireTimeUtc, Is.Not.Null);

    }
    
    [Test(Description = "Scheduler with 2 jobs that provide last run information")]
    public async Task JobRunDataTest()
    {
        IJobDetail successfulJob = JobBuilder.Create<TestSuccessfulQuartzJobWithInfo>()
            .WithIdentity("successfulJob", "group1")
            .Build();
        
        ITrigger successfulTrigger = TriggerBuilder.Create()
            .WithIdentity("successfulTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(5)
                .RepeatForever())
            .Build();
        
        IJobDetail failedJob = JobBuilder.Create<TestFailingQuartzJobWithInfo>()
            .WithIdentity("failedJob", "group1")
            .Build();
        
        ITrigger failedTrigger = TriggerBuilder.Create()
            .WithIdentity("failedTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(5)
                .RepeatForever())
            .Build();
        
        await _defaultScheduler.ScheduleJob(successfulJob, successfulTrigger);
        await _defaultScheduler.ScheduleJob(failedJob, failedTrigger);
        await _defaultScheduler.Start();
        Thread.Sleep(1000);

        using var host = await new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseActuatorQuartzEndpoint();
                    });
            }).StartAsync();

        var response = await host.GetTestClient().GetAsync(EndpointPath);
        
        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var responseObj = JsonConvert.DeserializeObject<QuartzEndpointResponse>(response.Content.ReadAsStringAsync().Result);
        
        Assert.That(responseObj.Schedulers.Count, Is.EqualTo(1));
        Assert.That(responseObj.Schedulers, Contains.Key("DefaultQuartzScheduler"));
        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].SchedulerStatus, Is.EqualTo("STARTED"));

        Assert.That(responseObj.Schedulers["DefaultQuartzScheduler"].Jobs.Count, Is.EqualTo(2));

        var failingJobObj = responseObj.Schedulers["DefaultQuartzScheduler"].Jobs
            .First(job => job.Name == "failedJob");
        
        var successfulJobObj = responseObj.Schedulers["DefaultQuartzScheduler"].Jobs
            .First(job => job.Name == "successfulJob");
        
        Assert.That(failingJobObj.LastRunSuccessful, Is.False);
        Assert.That(failingJobObj.LastRunErrorMessage, Is.EqualTo("error"));
        Assert.That(failingJobObj.LastErrorTimeUtc, Is.Not.Null);
        
        Assert.That(successfulJobObj.LastRunSuccessful, Is.True);
        Assert.That(successfulJobObj.LastRunErrorMessage, Is.Null);
        Assert.That(successfulJobObj.LastRunOutput, Is.EqualTo("stringOutput"));

    }
    
}