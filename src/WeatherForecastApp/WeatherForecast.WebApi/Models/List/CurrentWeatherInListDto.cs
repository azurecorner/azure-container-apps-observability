public class CurrentWeatherInListDto
{
    public string Time { get; set; }

    public int Interval { get; set; }

    public double Temperature { get; set; }

    public double Windspeed { get; set; }

    public int Winddirection { get; set; }

    public int IsDay { get; set; }

    public int Weathercode { get; set; }
}