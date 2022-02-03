using DotnetActuatorMiddleware;

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

// Tell quartz to schedule the job using our trigger
await scheduler.ScheduleJob(successJob, successTrigger);
await scheduler.ScheduleJob(failJob, failTrigger);

app.MapGet("/", () => "Hello World!");
app.UseActuatorQuartzEndpoint();

app.Run();

[PersistJobDataAfterExecution]
public class SuccessfulJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("Greetings from SuccessfulJob!");
        context.JobDetail.JobDataMap.Put("lastRunSuccessful", true);
    }
}

[PersistJobDataAfterExecution]
public class FailingJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("Greetings from FailingJob!");
        context.JobDetail.JobDataMap.Put("lastRunSuccessful", false);
        context.JobDetail.JobDataMap.Put("lastErrorMessage", "An error message");
    }
}