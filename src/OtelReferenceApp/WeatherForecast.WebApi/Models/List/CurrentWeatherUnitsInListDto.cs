public class CurrentWeatherUnitsInListDto
{
    public string Time { get; set; }

    public string Interval { get; set; }

    public string Temperature { get; set; }

    public string Windspeed { get; set; }

    public string Winddirection { get; set; }

    public string Weathercode { get; set; }

    public CurrentWeatherUnitsInListDto()
    {
        Time = "iso8601";
        Interval = "seconds";
        Temperature = "°C";
        Windspeed = "km/h";
        Winddirection = "°";
        Weathercode = "wmo code";
    }
}