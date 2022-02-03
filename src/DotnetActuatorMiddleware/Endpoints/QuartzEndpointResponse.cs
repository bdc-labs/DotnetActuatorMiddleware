namespace DotnetActuatorMiddleware.Endpoints;

public struct QuartzEndpointResponse
{
    public Dictionary<string, QuartzEndpointScheduler> Schedulers = new Dictionary<string, QuartzEndpointScheduler>();
}

public struct QuartzEndpointScheduler
{
    public string? SchedulerStatus;
    public List<QuartzEndpointJob> Jobs = new List<QuartzEndpointJob>();
}

public struct QuartzEndpointJob
{
    public string? Name;
    public string? Group;
    public string? Description;
    public string? JobClass;
    public bool? LastRunSuccessful = null;
    public string? LastRunErrorMessage = null;
    public bool ConcurrentExecutionAllowed;
    public bool PersistJobData;
    public List<QuartzEndpointJobTriggers> Triggers = new List<QuartzEndpointJobTriggers>();
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