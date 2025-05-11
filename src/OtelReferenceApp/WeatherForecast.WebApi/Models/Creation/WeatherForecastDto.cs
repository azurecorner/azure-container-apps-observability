using System.Text.Json.Serialization;

namespace WeatherForecast.WebApi.Models;

public class WeatherForecastForCreationDto
{
    [JsonPropertyName("department")]
    public string Department { get; set; }

    [JsonPropertyName("department_code")]
    public int DepartmentCode { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("postal_code")]
    public int PostalCode { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationtimeMs { get; set; }

    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }

    [JsonPropertyName("elevation")]
    public double Elevation { get; set; }

    //[JsonPropertyName("current_weather_units")]
    //public CurrentWeatherUnits CurrentWeatherUnits { get; set; }

    [JsonPropertyName("current_weather")]
    public CurrentWeather CurrentWeather { get; set; }
}