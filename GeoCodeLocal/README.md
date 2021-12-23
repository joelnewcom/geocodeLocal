## Run nominatim docker container with swiss OSM data

This spins up a nominatim server which downloads the osm file on startup. When we stop the container, all data is lost
```docker run -it --rm -e PBF_URL=https://download.geofabrik.de/europe/switzerland-latest.osm.pbf -e REPLICATION_URL=https://download.geofabrik.de/europe/switzerland-updates/ -p 8080:8080 --name nominatim mediagis/nominatim:3.7```

This spins up a nominatim server which downloads the osm file on startup. When we stop the container all data is stored in the docker volume. We can start a new container with the same command and it will take the data in the docker volume.
```docker run -it --rm --shm-size=1g -e PBF_URL=https://download.geofabrik.de/europe/switzerland-latest.osm.pbf -e REPLICATION_URL=https://download.geofabrik.de/europe/switzerland-updates/ -e IMPORT_WIKIPEDIA=false -v nominatim-data:/var/lib/postgresql/12/main -p 8080:8080 --name nominatim-persitent mediagis/nominatim:4.0```

## Run application
```dotnet run "./inputFile.csv" reset format1```
```dotnet run ./data/swiss-addresses.csv reset samzurcher```
```dotnet run ./data/10k-swiss-addresses.csv reset samzurcher```

## inputfile
inputFile is expected to have a header line.

## Params for nominatim

[Docu](https://nominatim.org/release-docs/latest/api/Search/)

http://localhost:8080/search?
street=<housenumber> <streetname>
city=<city>
county=<county>
state=<state>
country=<country>
postalcode=<postalcode>