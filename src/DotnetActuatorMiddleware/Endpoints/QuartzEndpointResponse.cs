namespace DotnetActuatorMiddleware.Endpoints;

public class QuartzEndpointResponse
{
    public Dictionary<string, QuartzEndpointScheduler> Schedulers = new Dictionary<string, QuartzEndpointScheduler>();
}

public class QuartzEndpointScheduler
{
    public string? SchedulerStatus;
    public List<QuartzEndpointJob> Jobs = [];
}

public class QuartzEndpointJob
{
    public string? Name;
    public string? Group;
    public string? Description;
    public string? JobClass;
    public bool? LastRunSuccessful = null;
    public DateTimeOffset? LastErrorTimeUtc;
    public string? LastRunErrorMessage = null;
    public object? LastRunOutput = null;
    public bool ConcurrentExecutionAllowed;
    public bool PersistJobData;
    public List<QuartzEndpointJobTriggers> Triggers = [];
}

public struct QuartzEndpointJobTriggers
{
    public string Name;
    public string Group;
    public string? Description;
    public DateTimeOffset? LastFireTimeUtc;
    public DateTimeOffset? NextFireTimeUtc;
    public DateTimeOffset? FinalFireTimeUtc;
    public DateTimeOffset StartTimeUtc;
    public DateTimeOffset? EndTimeUtc;
}