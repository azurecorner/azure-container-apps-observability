namespace WeatherForecast.Infrastructure.Models;

using System;
using System.Collections.Generic;

public class LocationInListViewModel
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
    public int Elevation { get; set; }
    public WeatherUnits WeatherUnits { get; set; }
    public List<CurrentWeather> CurrentWeather { get; set; }
}

public class WeatherUnits
{
    public string Time { get; set; }
    public string Interval { get; set; }
    public string Temperature { get; set; }
    public string Windspeed { get; set; }
    public string Winddirection { get; set; }
    public string Weathercode { get; set; }
}

public class CurrentWeather
{
    public DateTime Time { get; set; }
    public int Interval { get; set; }
    public double Temperature { get; set; }
    public double Windspeed { get; set; }
    public int Winddirection { get; set; }
    public int IsDay { get; set; }
    public int Weathercode { get; set; }
}