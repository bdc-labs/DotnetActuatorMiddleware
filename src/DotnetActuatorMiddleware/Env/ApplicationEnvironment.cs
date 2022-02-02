using System.Collections;
using System.Diagnostics;
using System.Net;
using DotnetActuatorMiddleware.Util;

namespace DotnetActuatorMiddleware.Env;

public class ApplicationEnvironment
{
    public int ProcessId;
    public DateTime ProcessStartTime;
    public double ProcessUptimeSecs;
    public string CommandLine;
    public string Hostname;
    public string Os;
    public string OsVersion;
    public string FrameworkVersion;
    public readonly Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();
    public readonly Dictionary<string, Dictionary<string, string>> ApplicationConfiguration = new Dictionary<string, Dictionary<string, string>>();
    internal readonly string LinuxDistroReleaseFile = "/etc/os-release";

    /// <summary>
    /// Gather details about the environment the application is running in
    /// <returns>Returns an instance of <see cref="ApplicationEnvironment"/></returns>
    /// </summary>
    public ApplicationEnvironment()
    {

        ProcessId = Environment.ProcessId;
        ProcessStartTime = Process.GetCurrentProcess().StartTime;
        ProcessUptimeSecs = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;
        CommandLine = Environment.CommandLine.Replace("\\", "\\\\");;
        Hostname = Dns.GetHostName();
        FrameworkVersion = Environment.Version.ToString();
        
        // Loop over environment variables to escape special characters
        IDictionary envVars = Environment.GetEnvironmentVariables();

        foreach (var envVar in envVars.Keys)
        {
            var envVarValue = envVars[envVar].ToString().Replace("\\", "\\\\").Replace('"', '\"');
            EnvironmentVariables.Add(envVar.ToString(), envVarValue);
        }
        
        OperatingSystem os = Environment.OSVersion;
        PlatformID platform = os.Platform;

        // Attempt to detect Linux distribution
        if (platform == PlatformID.Unix && File.Exists(LinuxDistroReleaseFile))
        {
            var distroInfo = DetectLinuxDistribution();
            Os = distroInfo.Name != null ? $"Linux ({distroInfo.Name})" : "Linux/Unix";
            OsVersion = distroInfo.Version ?? Environment.OSVersion.Version.ToString();
        }
        else
        {
            switch (platform)
            {
                case PlatformID.Unix:
                    Os = "Linux/Unix";
                    break;
                case PlatformID.Win32NT:
                    Os = "Windows";
                    break;
                case PlatformID.Win32Windows:
                    Os = "Windows";
                    break;
                case PlatformID.MacOSX: 
                    Os = "macOS";
                    break;
                default:
                    Os = "Unknown";
                    break;
            }
            
            OsVersion = Environment.OSVersion.Version.ToString();
        }

        // Loop over configuration sources and get their values
        foreach (var source in ConfigurationRegistry.Sources.Keys)
        {
            ApplicationConfiguration.Add(source, ConfigurationRegistry.GetAllValuesFromSource(source));
        }
    }

    internal LinuxDistribution DetectLinuxDistribution()
    {
        var releaseDetails = File.ReadAllLines(LinuxDistroReleaseFile);

        var distroName = releaseDetails.FirstOrDefault(s => s.StartsWith("NAME="))
            ?.Split("=")[1].Replace("\"", "");
        
        var distroVersion = releaseDetails.FirstOrDefault(s => s.StartsWith("VERSION_ID="))
            ?.Split("=")[1].Replace("\"", "");

        return new LinuxDistribution {Name = distroName, Version = distroVersion};
    }

    internal struct LinuxDistribution
    {
        public string? Name;
        public string? Version;
    }
    
}