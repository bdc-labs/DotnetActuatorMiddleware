using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace DotnetActuatorMiddleware.Endpoints;

internal class QuartzEndpoint : ActuatorEndpoint
{
    internal QuartzEndpoint(bool ipAllowListEnabled = false) : base(ipAllowListEnabled) { }

    internal QuartzEndpointResponse GetSchedulerStatus()
    {
        // Get all registered schedulers from Quartz
        StdSchedulerFactory factory = new StdSchedulerFactory();
        var registeredSchedulers = factory.GetAllSchedulers().Result;

        // Loop over all registered schedulers and get their details
        var schedulers = new Dictionary<string, QuartzEndpointScheduler>();
        foreach (var registeredScheduler in registeredSchedulers)
        {
            string schedulerStatus = "UNKNOWN";
            if (registeredScheduler.IsStarted)
            {
                schedulerStatus = "STARTED";
            }
            else if (registeredScheduler.IsShutdown)
            {
                schedulerStatus = "SHUTDOWN";
            }
            else if (registeredScheduler.InStandbyMode)
            {
                schedulerStatus = "STANDBY";
            }

            var jobKeys = registeredScheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Result;

            // Don't list this scheduler if it has no jobs
            if (jobKeys.Count == 0)
            {
                continue;
            }
            
            // Loop over all jobs in this scheduler instance and assemble their details
            List<QuartzEndpointJob> jobs = [];
            foreach (var jobKey in jobKeys)
            {
                var job = registeredScheduler.GetJobDetail(jobKey).Result;
                var jobTriggers = registeredScheduler.GetTriggersOfJob(jobKey).Result;

                if (job is null)
                {
                    continue;
                }

                // If a job has no attached triggers then it's probably not scheduled
                if (jobTriggers.Count == 0)
                {
                    continue;
                }

                var jobObj = new QuartzEndpointJob
                {
                    Name = jobKey.Name,
                    Group = jobKey.Group,
                    Description = job.Description,
                    ConcurrentExecutionAllowed = !job.ConcurrentExecutionDisallowed,
                    PersistJobData = job.PersistJobDataAfterExecution,
                    JobClass = job.JobType.FullName
                };

                // If the job sets the last lastRunSuccessful key and it's actually a bool then include it in our output
                if (job.JobDataMap.TryGetValue("lastRunSuccessful", out object? lastRunSuccessful) && lastRunSuccessful is bool)
                {
                    jobObj.LastRunSuccessful = (bool) lastRunSuccessful;
                }

                // If the job sets the last lastErrorMessage key and it's a non-null and non-empty string then include it in our output
                if (job.JobDataMap.TryGetValue("lastErrorMessage", out object? lastErrorMessage) && !string.IsNullOrWhiteSpace(lastErrorMessage.ToString()))
                {
                    jobObj.LastRunErrorMessage = lastErrorMessage.ToString();
                }
                
                // If the job sets the last lastErrorTimeUtc key and it's a DateTimeOffset then include it in our output
                if (job.JobDataMap.TryGetValue("lastErrorTimeUtc", out object? lastErrorTimeUtc) && lastErrorTimeUtc is DateTimeOffset)
                {
                    jobObj.LastErrorTimeUtc = (DateTimeOffset) lastErrorTimeUtc;
                }
                
                // Allow job to include an arbitrary output object that we'll serialize to JSON
                if (job.JobDataMap.TryGetValue("lastRunOutput", out object? lastRunOutput))
                {
                    jobObj.LastRunOutput = lastRunOutput;
                }

                // Get details all triggers for this job
                foreach (var trigger in jobTriggers)
                {
                    jobObj.Triggers.Add(new QuartzEndpointJobTriggers
                    {
                        Name = trigger.Key.Name,
                        Group = trigger.Key.Group,
                        LastFireTimeUtc = trigger.GetPreviousFireTimeUtc(),
                        NextFireTimeUtc = trigger.GetNextFireTimeUtc(),
                        FinalFireTimeUtc = trigger.FinalFireTimeUtc,
                        StartTimeUtc = trigger.StartTimeUtc,
                        EndTimeUtc = trigger.EndTimeUtc
                    });
                }
                
                jobs.Add(jobObj);
            }

            schedulers.Add(registeredScheduler.SchedulerName, new QuartzEndpointScheduler
            {
                SchedulerStatus = schedulerStatus,
                Jobs = jobs
            });
            
        }

        return new QuartzEndpointResponse
        {
            Schedulers = schedulers
        };

    }
}