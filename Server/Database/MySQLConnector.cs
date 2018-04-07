using MySql.Data.MySqlClient;

namespace Server.Database
{
    internal class MySqlConnector
    {

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection("datasource=localhost;port=3306;username=talky;password=talky;");
            connection.Open();
            connection.ChangeDatabase("talky");
            return connection;
        }
    }
}
