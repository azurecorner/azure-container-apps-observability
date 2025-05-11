public class WeatherData
{
    public string Department { get; set; }
    public int DepartmentCode { get; set; }
    public string City { get; set; }
    public int PostalCode { get; set; }
    public string Url { get; set; }

    public static List<WeatherData> IleDeFranceWeatherData => new List<WeatherData>
    {
        new WeatherData
        {
            Department = "Paris",
            DepartmentCode = 75,
            City = "Paris",
            PostalCode = 75000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.85&longitude=2.35&current_weather=true"
        },
        new WeatherData
        {
            Department = "Hauts-de-Seine",
            DepartmentCode = 92,
            City = "Nanterre",
            PostalCode = 92000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.892&longitude=2.206&current_weather=true"
        },
        new WeatherData
        {
            Department = "Seine-Saint-Denis",
            DepartmentCode = 93,
            City = "Bobigny",
            PostalCode = 93000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.91&longitude=2.45&current_weather=true"
        },
        new WeatherData
        {
            Department = "Val-de-Marne",
            DepartmentCode = 94,
            City = "Créteil",
            PostalCode = 94000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.79&longitude=2.455&current_weather=true"
        },
        new WeatherData
        {
            Department = "Val-d'Oise",
            DepartmentCode = 95,
            City = "Cergy",
            PostalCode = 95000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=49.038&longitude=2.08&current_weather=true"
        },
        new WeatherData
        {
            Department = "Yvelines",
            DepartmentCode = 78,
            City = "Versailles",
            PostalCode = 78000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.801&longitude=2.130&current_weather=true"
        },
        new WeatherData
        {
            Department = "Essonne",
            DepartmentCode = 91,
            City = "Évry-Courcouronnes",
            PostalCode = 91000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.632&longitude=2.441&current_weather=true"
        },
        new WeatherData
        {
            Department = "Seine-et-Marne",
            DepartmentCode = 77,
            City = "Melun",
            PostalCode = 77000,
            Url = "https://api.open-meteo.com/v1/forecast?latitude=48.542&longitude=2.655&current_weather=true"
        }
    };
}