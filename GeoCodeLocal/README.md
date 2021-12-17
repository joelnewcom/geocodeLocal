## Run nominatim docker container with swiss OSM data
First time:
```docker run -it --rm -e PBF_URL=https://download.geofabrik.de/europe/switzerland-latest.osm.pbf -e REPLICATION_URL=https://download.geofabrik.de/europe/switzerland-updates/ -p 8080:8080 --name nominatim mediagis/nominatim:3.7```

```docker run -it -e PBF_URL=https://download.geofabrik.de/europe/switzerland-latest.osm.pbf -e REPLICATION_URL=https://download.geofabrik.de/europe/switzerland-updates/ -p 8080:8080 --name nominatim mediagis/nominatim:3.7```

```docker run -it --rm --shm-size=1g -e PBF_URL=https://download.geofabrik.de/europe/switzerland-latest.osm.pbf -e REPLICATION_URL=https://download.geofabrik.de/europe/switzerland-updates/ -e IMPORT_WIKIPEDIA=false -v nominatim-data:/var/lib/postgresql/12/main -p 8080:8080 --name nominatim-persitent mediagis/nominatim:4.0```

## Run application
```dotnet run "./inputFile.csv"```

inputFile is expected to have a header line like: contactid,zch_street,zch_streetnumber,address1_postalcode,address1_city

## Params 

[Docu](https://nominatim.org/release-docs/latest/api/Search/)


http://localhost:8080/search?
street=<housenumber> <streetname>
city=<city>
county=<county>
state=<state>
country=<country>
postalcode=<postalcode>