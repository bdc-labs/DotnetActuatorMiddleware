using DotnetActuatorMiddleware;
using DotnetActuatorMiddleware.Util;

using Quartz;
using Quartz.Impl;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Start a Quartz scheduler
StdSchedulerFactory factory = new StdSchedulerFactory();
IScheduler scheduler = await factory.GetScheduler();
await scheduler.Start();

IJobDetail successJob = JobBuilder.Create<SuccessfulJob>()
    .WithIdentity("successfulJob", "group1")
    .Build();

IJobDetail failJob = JobBuilder.Create<FailingJob>()
    .WithIdentity("failingJob", "group1")
    .Build();

IJobDetail jobWithOutput = JobBuilder.Create<JobWithObject>()
    .WithIdentity("jobWithOutput", "group1")
    .Build();

ITrigger successTrigger = TriggerBuilder.Create()
    .WithIdentity("successTrigger", "group1")
    .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
    .WithSimpleSchedule(x => x
        .WithIntervalInSeconds(10)
        .RepeatForever())
    .Build();

ITrigger failTrigger = TriggerBuilder.Create()
    .WithIdentity("failTrigger", "group1")
    .StartNow()
    .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
    .WithSimpleSchedule(x => x
        .WithIntervalInSeconds(10)
        .RepeatForever())
    .Build();

ITrigger outputTrigger = TriggerBuilder.Create()
    .WithIdentity("outputTrigger", "group1")
    .StartNow()
    .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
    .WithSimpleSchedule(x => x
        .WithIntervalInSeconds(10)
        .RepeatForever())
    .Build();

// Tell quartz to schedule the job using our trigger
await scheduler.ScheduleJob(successJob, successTrigger);
await scheduler.ScheduleJob(failJob, failTrigger);
await scheduler.ScheduleJob(jobWithOutput, outputTrigger);

app.MapGet("/", () => "Hello World!");
app.UseActuatorQuartzEndpoint();

app.Run();

[PersistJobDataAfterExecution]
public class SuccessfulJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        context.MarkJobSuccessful();
        return Task.CompletedTask;
    }
}

[PersistJobDataAfterExecution]
public class FailingJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        context.MarkJobFailed("There was an error");
        return Task.CompletedTask;
    }
}

[PersistJobDataAfterExecution]
public class JobWithObject : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        context.MarkJobSuccessful(new { message = "Job succeeded" });
        return Task.CompletedTask;
    }
}