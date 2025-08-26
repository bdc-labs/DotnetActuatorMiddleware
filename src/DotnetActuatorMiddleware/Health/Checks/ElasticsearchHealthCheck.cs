using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

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
    public static HealthResponse CheckHealth(Uri[] servers, int timeoutSecs = 5, string? username = null, string? password = null, bool serverCertificateValidation = true)
    {
        try
        {
            var connectionPool = new StaticNodePool(servers);
            var clientSettings = new ElasticsearchClientSettings(connectionPool)
                .SniffOnStartup(false)
                .RequestTimeout(TimeSpan.FromSeconds(timeoutSecs))
                .PingTimeout(TimeSpan.FromSeconds(timeoutSecs));

            if (!serverCertificateValidation) {
                // If server certificate validation is disabled then always return true when calling the validation callback
                clientSettings.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => true);
            }

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password)) {
                clientSettings.Authentication(new BasicAuthentication(username, password));
            }
            
            
            var lowlevelClient = new ElasticsearchClient(clientSettings);

            var clusterHealthResponse = lowlevelClient.Cluster.Health();
            var healthResponse = new ElasticsearchHealthCheckResponse {
                ClusterName = clusterHealthResponse.ClusterName, Status = clusterHealthResponse.Status.ToString(),
            };

            if (!clusterHealthResponse.ApiCallDetails.HasSuccessfulStatusCode) {
                return clusterHealthResponse.ApiCallDetails.OriginalException is null ? HealthResponse.Unhealthy() : HealthResponse.Unhealthy(clusterHealthResponse.ApiCallDetails.OriginalException);
            }

            return clusterHealthResponse.Status != HealthStatus.Green ? HealthResponse.Unhealthy(healthResponse) : HealthResponse.Healthy(healthResponse);
        }
        catch (Exception e)
        {
            return HealthResponse.Unhealthy(e);
        }
    }
}