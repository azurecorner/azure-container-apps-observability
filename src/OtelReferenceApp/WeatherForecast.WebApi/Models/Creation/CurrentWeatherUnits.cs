using System.Text.Json.Serialization;

public class CurrentWeatherUnits
{
    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("interval")]
    public string Interval { get; set; }

    [JsonPropertyName("temperature")]
    public string Temperature { get; set; }

    [JsonPropertyName("windspeed")]
    public string Windspeed { get; set; }

    [JsonPropertyName("winddirection")]
    public string Winddirection { get; set; }

    [JsonPropertyName("is_day")]
    public string IsDay { get; set; }

    [JsonPropertyName("weathercode")]
    public string Weathercode { get; set; }

    public CurrentWeatherUnits()
    {
        Time = "iso8601";
        Interval = "seconds";
        Temperature = "°C";
        Windspeed = "km/h";
        Winddirection = "°";
        IsDay = string.Empty;
        Weathercode = "wmo code";
    }
}