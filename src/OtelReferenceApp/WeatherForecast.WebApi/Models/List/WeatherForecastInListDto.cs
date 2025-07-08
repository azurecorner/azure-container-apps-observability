namespace WeatherForecast.WebApi.Models.Creation;

public class WeatherForecastInListDto
{
    public int LocationId { get; set; }
    public string Department { get; set; }

    public int DepartmentCode { get; set; }

    public string City { get; set; }

    public int PostalCode { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double GenerationtimeMs { get; set; }

    public int UtcOffsetSeconds { get; set; }

    public string Timezone { get; set; }

    public string TimezoneAbbreviation { get; set; }

    public double Elevation { get; set; }

    public CurrentWeatherUnitsInListDto WeatherUnits { get; set; }
    public IEnumerable<CurrentWeatherInListDto> CurrentWeather { get; set; }
}