using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace GeoCodeLocal
{
    public class Store
    {
        private const string PREPARED_STMT_INSERT_RESULT = "INSERT INTO output (uuid, lat, long, address) VALUES ($uuid, $lat, $long, $address)";

        private const string PREPARED_STMT_INSERT_DAW_RESULT = "INSERT INTO dawoutput (Id, CustomerId, AddressHash, Longitude, Latitude, FullAddress, City, Street, PostalCode, HouseNumber, Region, CreatedDate, ModifiedDate, Country, Results) VALUES ($Id, $CustomerId, $AddressHash, $Longitude, $Latitude, $FullAddress, $City, $Street, $PostalCode, $HouseNumber, $Region, $CreatedDate, $ModifiedDate, $Country, $Results)";
        private const string PREPARED_STMT_INSERT_FAILURE = "INSERT INTO failures (uuid, address, reason, osm_type_ids) VALUES ($uuid, $address, $reason, $osm_type_ids)";
        SqliteConnection connection;
        SqliteCommand selectCommand;
        SqliteCommand insertCommand;
        SqliteParameter selectParam;

        public Store(Mode mode)
        {
            connection = new SqliteConnection("Data Source=store.db");
            connection.Open();

            if (!Mode.proceed.Equals(mode))
            {
                SqliteCommand dropOutputTableCommand = connection.CreateCommand();
                dropOutputTableCommand.CommandText = "DROP TABLE IF EXISTS output";
                dropOutputTableCommand.ExecuteNonQuery();

                SqliteCommand dropFailureTableCommand = connection.CreateCommand();
                dropFailureTableCommand.CommandText = "DROP TABLE IF EXISTS failures";
                dropFailureTableCommand.ExecuteNonQuery();

                SqliteCommand dropDawOutputTableCommand = connection.CreateCommand();
                dropDawOutputTableCommand.CommandText = "DROP TABLE IF EXISTS dawoutput";
                dropDawOutputTableCommand.ExecuteNonQuery();
            }

            // Id;CustomerId;AddressHash;Longitude;Latitude;FullAddress;City;Street;PostalCode;HouseNumber;Region;CreatedDate;ModifiedDate;Country;Results
            SqliteCommand createDawTableCommand = connection.CreateCommand();
            createDawTableCommand.CommandText = @"CREATE TABLE IF NOT EXISTS dawoutput (
                Id text, 
                CustomerId text, 
                AddressHash text,
                Longitude text,
                Latitude text,
                FullAddress text,
                City text,
                Street text,
                PostalCode text,
                HouseNumber text,
                Region text,
                CreatedDate DATETIME,
                ModifiedDate DATETIME,
                Country int,
                Results int)";
            createDawTableCommand.ExecuteNonQuery();


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
                    latParam.Value = entry.coordinate.lat;
                    lonParam.Value = entry.coordinate.lon;
                    addressParam.Value = entry.address;
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void saveBulk(List<DawCoordinateEntry> list)
        {
            using (var transaction = connection.BeginTransaction())
            {
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = PREPARED_STMT_INSERT_DAW_RESULT;

                // private const string PREPARED_STMT_INSERT_DAW_RESULT = "INSERT INTO dawoutput (Id, CustomerId, AddressHash, Longitude, Latitude, FullAddress, City, Street, PostalCode, HouseNumber, Region, CreatedDate, ModifiedDate, Country, Results) VALUES ($Id, $CustomerId, $AddressHash, $Longitude, $Latitude, $FullAddress, $City, $Street, $PostalCode, $HouseNumber, $Region, $CreatedDate, $ModifiedDate, $Country, $Results)";

                var idParam = insertCommand.CreateParameter();
                idParam.ParameterName = "$Id";
                insertCommand.Parameters.Add(idParam);

                var customerIdParam = insertCommand.CreateParameter();
                customerIdParam.ParameterName = "$CustomerId";
                insertCommand.Parameters.Add(customerIdParam);

                var addressHashParam = insertCommand.CreateParameter();
                addressHashParam.ParameterName = "$AddressHash";
                insertCommand.Parameters.Add(addressHashParam);

                var longitudeParam = insertCommand.CreateParameter();
                longitudeParam.ParameterName = "$Longitude";
                insertCommand.Parameters.Add(longitudeParam);

                var latitudeParam = insertCommand.CreateParameter();
                latitudeParam.ParameterName = "$Latitude";
                insertCommand.Parameters.Add(latitudeParam);

                var fullAddressParam = insertCommand.CreateParameter();
                fullAddressParam.ParameterName = "$FullAddress";
                insertCommand.Parameters.Add(fullAddressParam);

                var cityParam = insertCommand.CreateParameter();
                cityParam.ParameterName = "$City";
                insertCommand.Parameters.Add(cityParam);

                var streetParam = insertCommand.CreateParameter();
                streetParam.ParameterName = "$Street";
                insertCommand.Parameters.Add(streetParam);

                var postalCodeParam = insertCommand.CreateParameter();
                postalCodeParam.ParameterName = "$PostalCode";
                insertCommand.Parameters.Add(postalCodeParam);

                var houseNumberParam = insertCommand.CreateParameter();
                houseNumberParam.ParameterName = "$HouseNumber";
                insertCommand.Parameters.Add(houseNumberParam);

                var regionParam = insertCommand.CreateParameter();
                regionParam.ParameterName = "$Region";
                insertCommand.Parameters.Add(regionParam);

                var createdDateParam = insertCommand.CreateParameter();
                createdDateParam.ParameterName = "$CreatedDate";
                insertCommand.Parameters.Add(createdDateParam);

                var modifiedDateParam = insertCommand.CreateParameter();
                modifiedDateParam.ParameterName = "$ModifiedDate";
                insertCommand.Parameters.Add(modifiedDateParam);

                var countryParam = insertCommand.CreateParameter();
                countryParam.ParameterName = "$Country";
                insertCommand.Parameters.Add(countryParam);

                var resultsParam = insertCommand.CreateParameter();
                resultsParam.ParameterName = "$Results";
                insertCommand.Parameters.Add(resultsParam);

                foreach (DawCoordinateEntry entry in list)
                {
                    idParam.Value = entry.Id;
                    customerIdParam.Value = entry.CustomerId;
                    addressHashParam.Value = entry.AddressHash;
                    longitudeParam.Value = entry.Longitude;
                    latitudeParam.Value = entry.Latitude;
                    fullAddressParam.Value = entry.FullAddress;
                    cityParam.Value = entry.City;
                    streetParam.Value = entry.Street;
                    postalCodeParam.Value = entry.PostalCode;
                    houseNumberParam.Value = entry.HouseNumber;
                    regionParam.Value = entry.Region;
                    createdDateParam.Value = entry.CreatedDate;
                    modifiedDateParam.Value = entry.ModifiedDate;
                    countryParam.Value = entry.Country;
                    resultsParam.Value = entry.Results;
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