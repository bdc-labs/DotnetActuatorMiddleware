using System.Threading.Tasks;
using Quartz;

namespace DotnetActuatorMiddleware.Tests.Integration.Fixtures;

public class TestQuartzJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        return;
    }
}