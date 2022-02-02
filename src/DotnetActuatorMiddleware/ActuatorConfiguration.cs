using NetTools;

namespace DotnetActuatorMiddleware;

public static class ActuatorConfiguration
{
    /// <summary>
    /// List of IP ranges that are allowed to access the actuator endpoints
    /// </summary>
    internal static List<IPAddressRange> AllowedRanges = new();
    
    /// <summary>
    /// Clears and then sets the list of IPs allowed to access any actuator endpoints with IP restrictions enabled.
    /// </summary>
    /// <param name="allowedIps">A Comma-separated list of IPs, both single IPs and CIDRs can be used in the same string.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException">Thrown if any IPs in the string are invalid.</exception>
    public static void SetEndpointAllowedIps(string allowedIps)
    {
        if (String.IsNullOrEmpty(allowedIps)) throw new ArgumentNullException(nameof(allowedIps));
        
        var allowedIpRangeStrings = allowedIps.Split(",").ToList();
        var allowedRangesList = new List<IPAddressRange>();
        
        foreach (var allowedIpRangeString in allowedIpRangeStrings)
        {
            if (IPAddressRange.TryParse(allowedIpRangeString, out IPAddressRange allowedIpRange))
            {
                allowedRangesList.Add(allowedIpRange);
            }
            else
            {
                throw new InvalidOperationException($"Failed to parse IP range {allowedIpRangeString}");
            }
        }
        
        AllowedRanges.Clear();
        AllowedRanges = allowedRangesList;
    }
    
    /// <summary>
    /// Clears and then sets the list of IPs allowed to access any actuator endpoints with IP restrictions enabled.
    /// </summary>
    /// <param name="ipRanges">A list of <see cref="IPAddressRange"/> objects</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetEndpointAllowedIps(List<IPAddressRange> ipRanges)
    {
        if (ipRanges == null) throw new ArgumentNullException(nameof(ipRanges));

        if (ipRanges.Count == 0)
        {
            return;
        }

        AllowedRanges.Clear();
        AllowedRanges = ipRanges;
    }

    /// <summary>
    /// Remove all entries from the IP allow list
    /// </summary>
    public static void ClearIpAllowList()
    {
        AllowedRanges.Clear();
    }
}