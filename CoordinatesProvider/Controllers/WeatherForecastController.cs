using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CoordinatesProvider.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Coordinates")]
    public IEnumerable<Coordinate> Get(String commaSeperatedCustomerIds)
    {
        using (var connection = new SqliteConnection("Data Source=../GeoCodeLocal/store.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT CustomerId, Longitude, Latitude, FullAddress
                FROM dawoutput
                WHERE CustomerId IN (" + String.Join(",", commaSeperatedCustomerIds.Split(",").Select(s => "'" + s + "'")) + ")";

            // WHERE CustomerId IN ($ids);
            // command.Parameters.AddWithValue("$ids", String.Join(", ", customerIds.Select(s => "'" + s + "'")));

            string query = command.CommandText;

            // foreach (SqliteParameter p in command.Parameters)
            // {
            //     query = query.Replace(p.ParameterName, p.Value.ToString());
            // }

            Console.WriteLine(query);

            List<Coordinate> coordinates = new List<Coordinate>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    coordinates.Add(new Coordinate(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }
            }
            return coordinates;
        }

        return new List<Coordinate>();
    }
}
