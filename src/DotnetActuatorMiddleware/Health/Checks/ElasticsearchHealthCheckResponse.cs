using Elasticsearch.Net;

namespace DotnetActuatorMiddleware.Health.Checks;

public class ElasticsearchHealthCheckResponse
{
    public string ClusterName;
    public string Status;
}