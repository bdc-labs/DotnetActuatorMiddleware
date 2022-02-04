using System.Threading.Tasks;

using DotnetActuatorMiddleware.Util;

using Quartz;

namespace DotnetActuatorMiddleware.Tests.Integration.Fixtures;

[PersistJobDataAfterExecution]
public class TestSuccessfulQuartzJobWithInfo : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        context.MarkJobSuccessful("stringOutput");
    }
}