using GismeteoParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            var weatherForecast = parser.Parse();

            var dp = new DatabaseProcessor(
                userId: "root",
                password: "DB_USER_PASSWORD",
                databaseName: "gismeteo_weather_forecast",
                server: "localhost");

            dp.SaveWeatherForecast(weatherForecast);

            var cityWeatherForecast = new List<WeatherPoint>();
            var cities = dp.GetCities();

            if (cities.Count > 0)
            {
                var cityId = cities.First().Id.Value;

                var dates = dp.GetAvailableForecastDates(cityId);

                foreach (var date in dates)
                {
                    cityWeatherForecast.Add(dp.GetCityWeatherForecastOnDate(cityId, date));
                }
            }

            Console.WriteLine("Ready!");
            Console.ReadLine();
        }
    }
}
