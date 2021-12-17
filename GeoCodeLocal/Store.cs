using Microsoft.Data.Sqlite;

namespace GeoCodeLocal
{
    public class Store
    {
        SqliteConnection connection;
        public Store()
        {
            connection = new SqliteConnection("Data Source=store.db");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "drop table IF EXISTS output";
            command.ExecuteNonQuery();

            var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = "create table IF NOT EXISTS output (uuid varchar(20), lat varchar(20), long varchar(20))";
            dropCommand.ExecuteNonQuery();

        }

        internal String? getId(string id)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                    SELECT uuid
                    FROM output
                    WHERE uuid = $id";
            command.Parameters.AddWithValue("$id", id);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    return name;
                }
            }
            return null;
        }

        internal void save(string id, string lat, string lon)
        {
            var command = connection.CreateCommand();
            command.CommandText = "insert into output (uuid, lat, long) values ($uuid, $lat, $long)";
            command.Parameters.AddWithValue("$uuid", id);
            command.Parameters.AddWithValue("$lat", lat);
            command.Parameters.AddWithValue("$long", lon);
            command.ExecuteNonQuery();
        }

        public void Close(){
            connection.Close();
        }
    }
}