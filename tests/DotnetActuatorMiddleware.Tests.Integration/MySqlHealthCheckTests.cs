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
        
        Assert.True(checkResponse.IsHealthy);
        Assert.NotNull(checkResponse.Response);
        Assert.IsInstanceOf<MySqlHealthCheckResponse>(checkResponse.Response);
        
        var responseDetails = (MySqlHealthCheckResponse) checkResponse.Response!;
        
        Assert.NotNull(responseDetails.Host);
        Assert.NotNull(responseDetails.Version);
        Assert.AreEqual(3306, responseDetails.Port);
    }
    
    [Test(Description = "MySQL invalid credentials")]
    public void MysqlUnhealthyTest()
    {
        var checkResponse = MySqlHealthCheck.CheckHealth(InvalidCredentialConnString);
        
        Assert.False(checkResponse.IsHealthy);
        Assert.NotNull(checkResponse.Response);
        Assert.IsInstanceOf<String>(checkResponse.Response);
        Assert.True(checkResponse.Response!.ToString()!.StartsWith("EXCEPTION: "));
    }
    
    [Test(Description = "MySQL unreachable")]
    public void MysqlUnreachableTest()
    {
        var checkResponse = MySqlHealthCheck.CheckHealth(UnreachableConnString);
        
        Assert.False(checkResponse.IsHealthy);
        Assert.NotNull(checkResponse.Response);
        Assert.IsInstanceOf<String>(checkResponse.Response);
        Assert.True(checkResponse.Response!.ToString()!.StartsWith("EXCEPTION: "));
    }
}