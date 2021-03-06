using Elasticsearch.Net;
using Newtonsoft.Json;

namespace DotnetActuatorMiddleware.Health.Checks;

public static class ElasticsearchHealthCheck
{
    /// <summary>
    /// Check that a connection can be established to an Elasticsearch server or cluster and checks that server/cluster is healthy
    /// </summary>
    /// <param name="servers">An array of Elasticsearch server URLs</param>
    /// <param name="timeoutSecs">Elasticsearch request timeout in seconds</param>
    /// <param name="username">Elasticsearch username to authenticate with</param>
    /// <param name="password">Elasticsearch password to authenticate with</param>
    /// <param name="serverCertificateValidation">Whether or not to validate the SSL certificate returned by the server</param>
    /// <returns>A <see cref="HealthResponse"/> object that contains the return status of this health check</returns>
    public static HealthResponse CheckHealth(Uri[] servers, int timeoutSecs = 2, string? username = null, string? password = null, bool serverCertificateValidation = true)
    {
        try
        {
            var connectionPool = new StaticConnectionPool(servers);
            var settings = new ConnectionConfiguration(connectionPool)
                .SniffOnStartup(false)
                .RequestTimeout(TimeSpan.FromSeconds(timeoutSecs));

            if (!serverCertificateValidation)
            {
                // If server certificate validation is disabled then always return true when calling the validation callback
                settings.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => true);
            }

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                settings.BasicAuthentication(username, password);
            }
            
            var lowlevelClient = new ElasticLowLevelClient(settings);

            var clusterHealthResponse = lowlevelClient.Cluster.Health<StringResponse>();

            if (!clusterHealthResponse.Success)
            {
                return HealthResponse.Unhealthy(clusterHealthResponse.OriginalException);
            }

            var clusterHealthDetails = JsonConvert.DeserializeObject<ElasticsearchHealthCheckResponse>(clusterHealthResponse.Body);

            if (clusterHealthDetails!.Status == "red")
            {
                return HealthResponse.Unhealthy(new ElasticsearchHealthCheckResponse { ClusterName = clusterHealthDetails.ClusterName, Status = clusterHealthDetails.Status });
            }
            
            return HealthResponse.Healthy(new ElasticsearchHealthCheckResponse { ClusterName = clusterHealthDetails.ClusterName, Status = clusterHealthDetails.Status });
        }
        catch (Exception e)
        {
            return HealthResponse.Unhealthy(e);
        }
    }
}