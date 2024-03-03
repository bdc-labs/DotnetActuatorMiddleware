using System;
using DotnetActuatorMiddleware.Health.Checks;

using Elastic.Clients.Elasticsearch;

using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Integration;

[TestFixture]
public class ElasticsearchHealthCheckTests
{
    private readonly Uri[] _serverUris =
    {
        new Uri("http://localhost:19200/")
    };
    
    private readonly Uri[] _authServerUris =
    {
        new Uri("http://localhost:19201/")
    };
    
    private readonly Uri[] _sslServerUris =
    {
        new Uri("https://localhost:19202/")
    };
    
    private readonly Uri[] _invalidServerUris =
    {
        new Uri("http://es11:19200/"),
        new Uri("http://es12:19201/"),
        new Uri("http://es13:19202/")
    };
    
    [Test(Description = "Elasticsearch is healthy")]
    public void ElasticsearchHealthyTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_serverUris);
        
        Assert.That(elasticHealthCheck.IsHealthy, Is.True);
        Assert.That(elasticHealthCheck.Response, Is.Not.Null);
        Assert.That(elasticHealthCheck.Response, Is.InstanceOf<ElasticsearchHealthCheckResponse>());

        var elasticHealthCheckResponse = (ElasticsearchHealthCheckResponse) elasticHealthCheck.Response!;
        
        Assert.That(elasticHealthCheckResponse.Status, Is.EqualTo(HealthStatus.Green.ToString()));
    }
    
    [Test(Description = "Authenticated Elasticsearch health check")]
    public void ElasticsearchAuthenticationTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_authServerUris, username: "elastic", password: "changeme");
        
        Assert.That(elasticHealthCheck.IsHealthy, Is.True);
        Assert.That(elasticHealthCheck.Response, Is.Not.Null);
        Assert.That(elasticHealthCheck.Response, Is.InstanceOf<ElasticsearchHealthCheckResponse>());

        var elasticHealthCheckResponse = (ElasticsearchHealthCheckResponse) elasticHealthCheck.Response!;
        
        Assert.That(elasticHealthCheckResponse.Status, Is.EqualTo(HealthStatus.Green.ToString()));
    }
    
    [Test(Description = "SSL - Server certificate validation disabled")]
    public void ElasticsearchServerCertificateValidationDisabledTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_sslServerUris, username: "elastic", password: "changeme", serverCertificateValidation: false);
        
        Assert.That(elasticHealthCheck.IsHealthy, Is.True);
        Assert.That(elasticHealthCheck.Response, Is.Not.Null);
        Assert.That(elasticHealthCheck.Response, Is.InstanceOf<ElasticsearchHealthCheckResponse>());

        var elasticHealthCheckResponse = (ElasticsearchHealthCheckResponse) elasticHealthCheck.Response!;
        
        Assert.That(elasticHealthCheckResponse.Status, Is.EqualTo(HealthStatus.Green.ToString()));
    }
    
    [Test(Description = "SSL - Fail if certificate invalid")]
    public void ElasticsearchSelfSignedSslTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_sslServerUris, username: "elastic", password: "changeme");

        Assert.That(elasticHealthCheck.IsHealthy, Is.False);
    }
    
    [Test(Description = "Fail check if unauthenticated")]
    public void ElasticsearchUnauthenticatedTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_authServerUris);
        
        Assert.That(elasticHealthCheck.IsHealthy, Is.False);
    }
    
    [Test(Description = "Fail check if credentials are invalid")]
    public void ElasticsearchInvalidCredentialsTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_authServerUris, username: "elastic", password: "12345");
        
        Assert.That(elasticHealthCheck.IsHealthy, Is.False);
    }
    
    [Test(Description = "Elasticsearch is unreachable")]
    public void ElasticsearchUnreachableTest()
    {
        var elasticHealthCheck = ElasticsearchHealthCheck.CheckHealth(_invalidServerUris);
        
        Assert.That(elasticHealthCheck.IsHealthy, Is.False);
        Assert.That(elasticHealthCheck.Response, Is.Not.Null);
        Assert.That(elasticHealthCheck.Response, Is.InstanceOf<String>());
        Assert.That(elasticHealthCheck.Response!.ToString()!.StartsWith("EXCEPTION: "), Is.True);
    }
}