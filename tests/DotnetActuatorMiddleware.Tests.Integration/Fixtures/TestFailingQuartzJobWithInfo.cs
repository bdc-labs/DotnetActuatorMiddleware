using System.Threading.Tasks;
using DotnetActuatorMiddleware.Util;
using Quartz;

namespace DotnetActuatorMiddleware.Tests.Integration.Fixtures;

[PersistJobDataAfterExecution]
public class TestFailingQuartzJobWithInfo : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        context.MarkJobFailed("error");
        return Task.CompletedTask;
    }
}