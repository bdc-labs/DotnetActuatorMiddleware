using System.Threading.Tasks;

using Quartz;

namespace DotnetActuatorMiddleware.Tests.Integration.Fixtures;

[PersistJobDataAfterExecution]
public class TestSuccessfulQuartzJobWithInfo : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        context.JobDetail.JobDataMap.Put("lastRunSuccessful", true);
    }
}