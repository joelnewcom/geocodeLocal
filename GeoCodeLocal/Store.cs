using Microsoft.Data.Sqlite;

namespace GeoCodeLocal
{
    public class Store
    {
        private const string PREPARED_STMT_INSERT_RESULT = "INSERT INTO output (uuid, lat, long, address) VALUES ($uuid, $lat, $long, $address)";
        private const string PREPARED_STMT_INSERT_FAILURE = "INSERT INTO failures (uuid, address, reason, osm_type_ids) VALUES ($uuid, $address, $reason, $osm_type_ids)";
        SqliteConnection connection;
        SqliteCommand selectCommand;
        SqliteCommand insertCommand;

        SqliteParameter selectParam;

        public Store(string mode)
        {
            connection = new SqliteConnection("Data Source=store.db");
            connection.Open();

            if (!"proceed".Equals(mode))
            {
                SqliteCommand dropOutputTableCommand = connection.CreateCommand();
                dropOutputTableCommand.CommandText = "DROP TABLE IF EXISTS output";
                dropOutputTableCommand.ExecuteNonQuery();
            }

            if (!"proceed".Equals(mode))
            {
                SqliteCommand dropFailureTableCommand = connection.CreateCommand();
                dropFailureTableCommand.CommandText = "DROP TABLE IF EXISTS failures";
                dropFailureTableCommand.ExecuteNonQuery();
            }

            SqliteCommand createOutputTableCommand = connection.CreateCommand();
            createOutputTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS output (uuid varchar(20), address varchar(20), lat varchar(20), long varchar(20))";
            createOutputTableCommand.ExecuteNonQuery();

            SqliteCommand createIndexOnOutputTableCommand = connection.CreateCommand();
            createIndexOnOutputTableCommand.CommandText = "CREATE INDEX IF NOT EXISTS uuidIndex ON output (uuid)";
            createIndexOnOutputTableCommand.ExecuteNonQuery();

            SqliteCommand createFailureTableCommand = connection.CreateCommand();
            createFailureTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS failures (uuid varchar(20), address varchar(20), reason varchar(20), osm_type_ids varchar(20))";
            createFailureTableCommand.ExecuteNonQuery();

            selectCommand = connection.CreateCommand();
            selectCommand.CommandText = @"
                    SELECT uuid
                    FROM output
                    WHERE uuid = $id
                    LIMIT 1";
            selectParam = selectCommand.CreateParameter();
            selectParam.ParameterName = "$id";
            selectCommand.Parameters.Add(selectParam);

            insertCommand = connection.CreateCommand();
            insertCommand.CommandText = PREPARED_STMT_INSERT_RESULT;
        }

        internal String? getId(string id)
        {
            if (id is null)
            {
                return null;
            }

            selectParam.Value = id;
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

        public void saveBulk(List<CoordinateEntry> list)
        {
            using (var transaction = connection.BeginTransaction())
            {

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = PREPARED_STMT_INSERT_RESULT;

                var uuidParam = insertCommand.CreateParameter();
                uuidParam.ParameterName = "$uuid";
                insertCommand.Parameters.Add(uuidParam);

                var latParam = insertCommand.CreateParameter();
                latParam.ParameterName = "$lat";
                insertCommand.Parameters.Add(latParam);

                var lonParam = insertCommand.CreateParameter();
                lonParam.ParameterName = "$long";
                insertCommand.Parameters.Add(lonParam);

                var addressParam = insertCommand.CreateParameter();
                addressParam.ParameterName = "$address";
                insertCommand.Parameters.Add(addressParam);

                foreach (CoordinateEntry entry in list)
                {
                    uuidParam.Value = entry.uuid;
                    latParam.Value = entry.lat;
                    lonParam.Value = entry.lon;
                    addressParam.Value = entry.address;
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void saveFailures(List<FailureEntry> list)
        {
            using (var transaction = connection.BeginTransaction())
            {
                //CREATE TABLE IF NOT EXISTS failures (uuid varchar(20), address varchar(20), reason varchar(20), osm_type_ids varchar(20)))
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = PREPARED_STMT_INSERT_FAILURE;
                var uuidParam = insertCommand.CreateParameter();
                var addressParam = insertCommand.CreateParameter();
                var reasonParam = insertCommand.CreateParameter();
                var osmIdsParam = insertCommand.CreateParameter();

                uuidParam.ParameterName = "$uuid";
                insertCommand.Parameters.Add(uuidParam);

                addressParam.ParameterName = "$address";
                insertCommand.Parameters.Add(addressParam);

                reasonParam.ParameterName = "$reason";
                insertCommand.Parameters.Add(reasonParam);

                osmIdsParam.ParameterName = "$osm_type_ids";
                insertCommand.Parameters.Add(osmIdsParam);

                foreach (FailureEntry entry in list)
                {
                    uuidParam.Value = entry.uuid;
                    addressParam.Value = entry.address;
                    reasonParam.Value = entry.reason;
                    osmIdsParam.Value = entry.osmTypeIds;
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