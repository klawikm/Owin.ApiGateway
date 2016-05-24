namespace Owin.ApiGateway.DbConfigurationStorageService
{
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;

    using Owin.ApiGateway.Common;

    public class ConfigurationStorageService : IConfigurationStorageService
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ConfigurationStorageService"].ConnectionString;
            }
        }

        public string Read()
        {
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT TOP 1 Content FROM Configuration ORDER BY Id DESC", conn))
                {
                    return (string)cmd.ExecuteScalar();
                }
            }
        }

        public void Write(string configurationString)
        {
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Configuration (Content) VALUES (@Content)", conn);
                cmd.Parameters.Add("Content", SqlDbType.VarChar).Value = configurationString;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
