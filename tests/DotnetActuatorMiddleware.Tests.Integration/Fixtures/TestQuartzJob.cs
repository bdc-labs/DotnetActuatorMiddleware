using System.Threading.Tasks;
using Quartz;

namespace DotnetActuatorMiddleware.Tests.Integration.Fixtures;

public class TestQuartzJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}