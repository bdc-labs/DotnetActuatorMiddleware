using System.Threading.Tasks;
using Quartz;

namespace DotnetActuatorMiddleware.Tests.Integration.Fixtures;

[PersistJobDataAfterExecution]
public class TestFailingQuartzJobWithInfo : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        context.JobDetail.JobDataMap.Put("lastRunSuccessful", false);
        context.JobDetail.JobDataMap.Put("lastErrorMessage", "error");
    }
}