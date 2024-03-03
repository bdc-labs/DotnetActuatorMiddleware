using System;
using DotnetActuatorMiddleware.Health.Checks;
using NUnit.Framework;

namespace DotnetActuatorMiddleware.Tests.Integration;

[TestFixture]
public class MySqlHealthCheckTests
{
    private const string ValidConnString = "Server=localhost;Port=19306;Database=testdb;Uid=root;Pwd=r00t;Connection Timeout=2;";
    private const string UnreachableConnString = "Server=unreachablemysql;Port=19306;Database=testdb;Uid=root;Pwd=r00t;Connection Timeout=2;";
    private const string InvalidCredentialConnString = "Server=localhost;Port=19306;Database=testdb;Uid=root;Pwd=root;Connection Timeout=2;";

    [Test(Description = "MySQL healthy")]
    public void MysqlHealthyTest()
    {
        var checkResponse = MySqlHealthCheck.CheckHealth(ValidConnString);
        
        Assert.That(checkResponse.IsHealthy, Is.True);
        Assert.That(checkResponse.Response, Is.Not.Null);
        Assert.That(checkResponse.Response, Is.InstanceOf<MySqlHealthCheckResponse>());
        
        var responseDetails = (MySqlHealthCheckResponse) checkResponse.Response!;
        
        Assert.That(responseDetails.Host, Is.Not.Null);
        Assert.That(responseDetails.Version, Is.Not.Null);
        Assert.That(responseDetails.Port, Is.EqualTo(3306));
    }
    
    [Test(Description = "MySQL invalid credentials")]
    public void MysqlUnhealthyTest()
    {
        var checkResponse = MySqlHealthCheck.CheckHealth(InvalidCredentialConnString);
        
        Assert.That(checkResponse.IsHealthy, Is.False);
        Assert.That(checkResponse.Response, Is.Not.Null);
        Assert.That(checkResponse.Response, Is.InstanceOf<String>());
        Assert.That(checkResponse.Response!.ToString()!.StartsWith("EXCEPTION: "), Is.True);
    }
    
    [Test(Description = "MySQL unreachable")]
    public void MysqlUnreachableTest()
    {
        var checkResponse = MySqlHealthCheck.CheckHealth(UnreachableConnString);
        
        Assert.That(checkResponse.IsHealthy, Is.False);
        Assert.That(checkResponse.Response, Is.Not.Null);
        Assert.That(checkResponse.Response, Is.InstanceOf<String>());
        Assert.That(checkResponse.Response!.ToString()!.StartsWith("EXCEPTION: "), Is.True);
    }
}