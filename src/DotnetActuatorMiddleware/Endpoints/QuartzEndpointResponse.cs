namespace DotnetActuatorMiddleware.Endpoints;

public class QuartzEndpointResponse
{
    public Dictionary<string, QuartzEndpointScheduler> Schedulers { get; set; } = new Dictionary<string, QuartzEndpointScheduler>();
}

public class QuartzEndpointScheduler
{
    public string? SchedulerStatus { get; set; }
    public List<QuartzEndpointJob> Jobs { get; set; } = [];
}

public class QuartzEndpointJob
{
    public string? Name { get; set; }
    public string? Group { get; set; }
    public string? Description { get; set; }
    public string? JobClass { get; set; }
    public bool? LastRunSuccessful { get; set; }
    public DateTimeOffset? LastErrorTimeUtc { get; set; }
    public string? LastRunErrorMessage { get; set; }
    public object? LastRunOutput { get; set; }
    public bool ConcurrentExecutionAllowed { get; set; }
    public bool PersistJobData { get; set; }
    public List<QuartzEndpointJobTriggers> Triggers  { get; set; } = [];
}

public struct QuartzEndpointJobTriggers
{
    public string Name { get; set; }
    public string Group { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? LastFireTimeUtc { get; set; }
    public DateTimeOffset? NextFireTimeUtc { get; set; }
    public DateTimeOffset? FinalFireTimeUtc { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public DateTimeOffset? EndTimeUtc { get; set; }
}