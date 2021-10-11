using System;

namespace GismeteoParser
{
    public class WeatherPoint
    {
        /// <summary>
        /// City name
        /// </summary>
        public String CityName { get; set; }

        /// <summary>
        /// Path
        /// </summary>
        public String Path { get; set; }

        /// <summary>
        /// Forecast date
        /// </summary>
        public DateTime ForecastDate { get; set; }

        /// <summary>
        /// Weather forecast parsed date and time
        /// </summary>
        public DateTime ParsedAt { get; set; }

        /// <summary>
        /// Minimum temperature in Celsius degree
        /// </summary>
        public short MinTempCelsius { get; set; }

        /// <summary>
        /// Minimum temperature in Fahrenheit degree
        /// </summary>
        public short MinTempFahrenheit { get; set; }

        /// <summary>
        /// Maximum temperature in Celsius degree
        /// </summary>
        public short MaxTempCelsius { get; set; }

        /// <summary>
        /// Maximum temperature in Fahrenheit degree
        /// </summary>
        public short MaxTempFahrenheit { get; set; }

        /// <summary>
        /// Maximum wind speed in meters per second
        /// </summary>
        public ushort MaxWindSpeedMetersPerSecond { get; set; }

        /// <summary>
        /// Maximum wind speed in miles per hour
        /// </summary>
        public ushort MaxWindSpeedMilesPerHour { get; set; }

        /// <summary>
        /// Maximum wind speed in kilometers per hour
        /// </summary>
        public ushort MaxWindSpeedKilometersPerHour { get; set; }

        /// <summary>
        /// Precipitation amount in millimeters
        /// </summary>
        public float? PrecipitationAmount { get; set; }

        /// <summary>
        /// Weather forecast summary
        /// </summary>
        public string Summary { get; set; }

        public WeatherPoint()
        { }
    }
}