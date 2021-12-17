using Microsoft.Data.Sqlite;

namespace GeoCodeLocal
{
    public class Store
    {
        SqliteConnection connection;
        SqliteCommand createTableCommand;
        SqliteCommand dropTableCommand;
        SqliteCommand selectCommand;
        SqliteCommand insertCommand;

        public Store()
        {
            connection = new SqliteConnection("Data Source=store.db");
            connection.Open();
            dropTableCommand = connection.CreateCommand();
            dropTableCommand.CommandText = "DROP TABLE IF EXISTS output";
            dropTableCommand.ExecuteNonQuery();

            createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS output (uuid varchar(20), lat varchar(20), long varchar(20))";
            createTableCommand.ExecuteNonQuery();

            selectCommand = connection.CreateCommand();
            selectCommand.CommandText = @"
                    SELECT uuid
                    FROM output
                    WHERE uuid = $id";
            insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO output (uuid, lat, long) VALUES ($uuid, $lat, $long)";
        }

        internal String? getId(string id)
        {
            selectCommand.Parameters.AddWithValue("$id", id);

            using (var reader = selectCommand.ExecuteReader())
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
            insertCommand.Parameters.AddWithValue("$uuid", id);
            insertCommand.Parameters.AddWithValue("$lat", lat);
            insertCommand.Parameters.AddWithValue("$long", lon);
            insertCommand.ExecuteNonQuery();
        }

        public void bulkSave(List<CoordinateEntry> list)
        {

            using (var transaction = connection.BeginTransaction())
            {

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "INSERT INTO output (uuid, lat, long) VALUES ($uuid, $lat, $long)";

                var uuidParam = insertCommand.CreateParameter();
                uuidParam.ParameterName = "$uuid";
                insertCommand.Parameters.Add(uuidParam);

                var latParam = insertCommand.CreateParameter();
                latParam.ParameterName = "$lat";
                insertCommand.Parameters.Add(latParam);

                var lonParam = insertCommand.CreateParameter();
                lonParam.ParameterName = "$long";
                insertCommand.Parameters.Add(lonParam);

                foreach (CoordinateEntry entry in list)
                {
                    uuidParam.Value = entry.uuid;
                    latParam.Value = entry.lat;
                    lonParam.Value = entry.lon;
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void Close()
        {
            connection.Close();
        }
    }
}