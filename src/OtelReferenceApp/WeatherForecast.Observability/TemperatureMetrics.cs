
namespace Weather.Infrastructure.Metrics
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;

    namespace Weather.Infrastructure.Metrics
    {
        public class TemperatureMetrics
        {
            private readonly Meter _meter;

            // Counter
            private readonly Counter<int> WeatherReportsCounter;

            // Histogram
            private readonly Histogram<double> TemperatureHistogram;

            private readonly ObservableGauge<double> CurrentTemperatureGauge;

            // 👇 User login metrics
            private readonly Counter<int> LoginSuccessCounter;
            private readonly Counter<int> LoginFailureCounter;

            // Internal state
            private readonly Dictionary<string, double> _currentTemperatures = new();
            public TemperatureMetrics(IMeterFactory meterFactory, IConfiguration configuration)
            {
                var meterName = configuration["TemperatureMeterName"]
                                ?? throw new ArgumentNullException(nameof(configuration), "TemperatureMeterName is missing in configuration.");
                _meter = meterFactory.Create(meterName);

                // Initialize counter
                WeatherReportsCounter = _meter.CreateCounter<int>(
                    "weather-reports",
                    "reports",
                    "Number of weather reports received per city"
                );

                // Initialize Histogram
                TemperatureHistogram = _meter.CreateHistogram<double>(
                      "temperature-distribution",
                      "celsius",
                      "Recorded temperature values per city"
                  );

                // ObservableGauge: current/latest temperature per city
                CurrentTemperatureGauge = _meter.CreateObservableGauge<double>(
                    "current_temperature_celsius",
                    () =>
                    {
                        var measurements = new List<Measurement<double>>();
                        foreach (var kvp in _currentTemperatures)
                        {
                            measurements.Add(new Measurement<double>(
                                kvp.Value,
                                KeyValuePair.Create<string, object?>("City", kvp.Key) 
                            ));
                        }
                        return measurements;
                    },
                    unit: "celsius",
                    description: "Current temperature per city"
                );


                // Initialize login metrics
                LoginSuccessCounter = _meter.CreateCounter<int>(
                    "user_login_success",
                    unit: "attempts",
                    description: "Counts number of successful user login attempts"
                );

                LoginFailureCounter = _meter.CreateCounter<int>(
                    "user_login_failed",
                    unit: "attempts",
                    description: "Counts number of failed user login attempts"
                );
            }

            /// <summary>
            /// Increments the weather report count for a specific city.
            /// </summary>
            /// <param name="city">The city name where the weather report was received.</param>
            public void IncrementWeatherReport(string city)
            {
                if (string.IsNullOrWhiteSpace(city))
                    throw new ArgumentException("City name cannot be null or empty.", nameof(city));

                WeatherReportsCounter.Add(1, KeyValuePair.Create<string, object?>(
                    "City", city 
                ));
            }

            /// <summary>
            /// Records a temperature value for a given city in the histogram.
            /// </summary>
            public void RecordTemperature(string city, double temperature)
            {
                if (string.IsNullOrWhiteSpace(city))
                    throw new ArgumentException("City name cannot be null or empty.", nameof(city));

                TemperatureHistogram.Record(temperature, KeyValuePair.Create<string, object?>("City", city)); 
            }

            /// <summary>
            /// Updates the current/latest temperature for a given city.
            /// </summary>
            public void UpdateCurrentTemperature(string city, double temperature)
            {
                if (string.IsNullOrWhiteSpace(city))
                    throw new ArgumentException("City name cannot be null or empty.", nameof(city));

                _currentTemperatures[city] = temperature;
            }

            public void TrackLoginSuccess(string username)
            {
                LoginSuccessCounter.Add(1, new KeyValuePair<string, object?>("username", username));
            }

            public void TrackLoginFailure(string username)
            {
                LoginFailureCounter.Add(1, new KeyValuePair<string, object?>("username", username));
            }

        }
    }

}
