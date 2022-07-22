using MySql.Data.MySqlClient;

namespace DotnetActuatorMiddleware.Health.Checks;

public static class MySqlHealthCheck
{
    /// <summary>
    /// Check that a connection can be established to MySQL and return the server version, hostname and client port
    /// </summary>
    /// <param name="connectionString">A MySQL connection string</param>
    /// <returns>A <see cref="HealthResponse"/> object that contains the return status of this health check</returns>
    public static HealthResponse CheckHealth(string connectionString)
    {
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand())
            {
                conn.Open();
                cmd.Connection = conn;

                cmd.CommandText = "SELECT @@version AS version, @@port AS port, @@hostname AS hostname";
                var reader = cmd.ExecuteReader();
                reader.Read();

                return HealthResponse.Healthy(new MySqlHealthCheckResponse { Host = reader.GetString("hostname"), Port = reader.GetInt32("port") , Version = reader.GetString("version")});
                
            }
        }
        catch (Exception e)
        {
            return HealthResponse.Unhealthy(e);
        }
    }
}