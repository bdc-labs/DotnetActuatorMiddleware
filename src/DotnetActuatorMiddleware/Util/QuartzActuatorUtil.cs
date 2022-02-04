using Quartz;

namespace DotnetActuatorMiddleware.Util;

/// <summary>
/// Extension class that provides helpers for informing the Quartz endpoint of a job's execution status
/// </summary>
public static class QuartzActuatorUtil
{
    /// <summary>
    /// Mark job as failed with appropriate metadata for the Quartz status actuator endpoint
    /// </summary>
    /// <param name="context"></param>
    /// <remarks>
    ///     Marks job as failed by adding the key <b>lastRunSuccessful</b> to the JobDataMap with a boolean value of false and a key named <b>lastErrorTimeUtc</b>
    ///     containing the UTC time of the failure as a DateTimeOffset object.
    /// </remarks>
    public static void MarkJobFailed(this IJobExecutionContext context)
    {
        context.MarkJobFailed(null, null);
    }

    /// <summary>
    /// Mark job as failed with appropriate metadata for the Quartz status actuator endpoint
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errorMessage">An error message string to include in the status output, this will be set as <b>lastErrorMessage</b> in the Quartz JobDataMap</param>
    /// <remarks>
    ///     Marks job as failed by adding the key <b>lastRunSuccessful</b> to the Quartz JobDataMap with a boolean value of false and a key named <b>lastErrorTimeUtc</b>
    ///     containing the UTC time of the failure as a DateTimeOffset object.
    /// </remarks>
    public static void MarkJobFailed(this IJobExecutionContext context, string? errorMessage)
    {
        context.MarkJobFailed(errorMessage, null);
    }

    /// <summary>
    /// Mark job as failed with appropriate metadata for the Quartz status actuator endpoint
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errorMessage">An error message string to include in the status output, this will be set as <b>lastErrorMessage</b> in the Quartz JobDataMap</param>
    /// <param name="outputObject">An object that will be serialized to JSON and included in the status output, this will be set as <b>lastRunOutput</b> in the Quartz JobDataMap</param>
    /// <remarks>Marks job as failed by adding the key <b>lastRunSuccessful</b> to the JobDataMap with a boolean value of false along with a key named <b>lastErrorMessage</b></remarks>
    public static void MarkJobFailed(this IJobExecutionContext context, string? errorMessage, object? outputObject)
    {
        context.JobDetail.JobDataMap.Put("lastRunSuccessful", false);
        context.JobDetail.JobDataMap.Put("lastErrorTimeUtc", DateTimeOffset.UtcNow);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            context.JobDetail.JobDataMap.Put("lastErrorMessage", errorMessage);
        }
        
        if (outputObject is not null)
        {
            context.JobDetail.JobDataMap.Put("lastRunOutput", outputObject);
        }
    }
    
    /// <summary>
    /// Mark job as successful with appropriate metadata for the Quartz status actuator endpoint
    /// </summary>
    /// <param name="context"></param>
    /// <remarks>
    ///     Marks job as failed by adding the key <b>lastRunSuccessful</b> to the JobDataMap with a boolean value of false and a key named <b>lastErrorTimeUtc</b>
    ///     containing the UTC time of the failure as a DateTimeOffset object.
    /// </remarks>
    public static void MarkJobSuccessful(this IJobExecutionContext context)
    {
        context.MarkJobSuccessful(null);
    }
    
    /// <summary>
    /// Mark job as successful with appropriate metadata for the Quartz status actuator endpoint
    /// </summary>
    /// <param name="context"></param>
    /// <param name="outputObject">An object that will be serialized to JSON and included in the status output, this will be set as <b>lastRunOutput</b> in the Quartz JobDataMap</param>
    /// <remarks>Marks job as successful by adding the key <b>lastRunSuccessful</b> to the JobDataMap with a boolean value of true</remarks>
    public static void MarkJobSuccessful(this IJobExecutionContext context, object? outputObject)
    {
        context.JobDetail.JobDataMap.Put("lastRunSuccessful", true);

        if (outputObject is not null)
        {
            context.JobDetail.JobDataMap.Put("lastRunOutput", outputObject);
        }
    }

}