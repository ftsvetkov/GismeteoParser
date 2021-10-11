using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GismeteoParser
{
    public class DatabaseProcessor
    {
        private readonly MySqlConnection connection;
        private MySqlConnection Connection
        {
            get
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                return connection;
            }
        }

        public DatabaseProcessor(
            string password,
            string userId = "root",
            string databaseName = "gismeteo_weather_forecast",
            string server = "localhost")
        {
            connection = 
                new MySqlConnection($"Server={server};User ID={userId};Password={password};Database={databaseName}");

            InitDatabase(password, userId, databaseName, server);
        }

        ~DatabaseProcessor()
        {
            connection.Close();
        }

        public void SaveWeatherForecast(Dictionary<string, List<WeatherPoint>> weatherForecast)
        {
            if (weatherForecast is null)
            {
                throw new ArgumentNullException(nameof(weatherForecast));
            }

            // Get cities
            var databaseCities = GetCities();
            var databaseCitiesPaths = databaseCities.Select(c => c.Path);

            foreach (KeyValuePair<string, List<WeatherPoint>> cityWeatherForecast in weatherForecast)
            {
                int? cityId = null;

                // Save new cities
                if (!databaseCitiesPaths.Contains(cityWeatherForecast.Key))
                {
                    var firstWeatherPoint = cityWeatherForecast.Value.First();
                    cityId = SaveCity(firstWeatherPoint.CityName, firstWeatherPoint.Path);

                    if (cityId > 0)
                    {
                        databaseCities.Add(new City(firstWeatherPoint.CityName, firstWeatherPoint.Path, cityId));
                    }
                }

                // Save weather points
                foreach (var weatherPoint in cityWeatherForecast.Value)
                {
                    SaveWeatherPoint(weatherPoint, cityId ?? 
                        databaseCities.Where(c => c.Path == cityWeatherForecast.Key).Select(c => c.Id.Value).First());
                }
            }
        }

        public List<City> GetCities()
        {
            var cities = new List<City>();

            using (var command = new MySqlCommand("SELECT * FROM city;", Connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cities.Add(new City(
                            reader.GetString("name"),
                            reader.GetString("path"),
                            reader.GetInt32("city_id")));
                    }
                }
            }

            return cities;
        }

        public List<DateTime> GetAvailableForecastDates(int cityId)
        {
            var dates = new List<DateTime>();

            var query =
            $@"SELECT forecast_date FROM weather_point
               WHERE 
                 city_id = {cityId} AND 
                 parsed_at IN (
                   SELECT MAX(parsed_at) FROM weather_point WHERE city_id = {cityId});";

            using (var command = new MySqlCommand(query, Connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dates.Add(reader.GetDateTime("forecast_date"));
                    }
                }
            }

            return dates;
        }

        public WeatherPoint GetCityWeatherForecastOnDate(int cityId, DateTime forecastDate)
        {
            WeatherPoint weatherPoint = null;

            var query =
            @"SELECT c.name, c.path, wp.* FROM weather_point as wp
              INNER JOIN city as c ON wp.city_id = c.city_id
              WHERE
                wp.city_id = @city_id AND
                wp.forecast_date = @forecast_date AND
                wp.parsed_at IN (
                  SELECT MAX(parsed_at) FROM weather_point WHERE city_id = @city_id);";

            using (var command = new MySqlCommand(query, Connection))
            {
                var cityIdParam = new MySqlParameter("@city_id", MySqlDbType.UInt32);
                cityIdParam.Direction = System.Data.ParameterDirection.Input;
                cityIdParam.Value = cityId;
                command.Parameters.Add(cityIdParam);

                var forecastDateParam = new MySqlParameter("@forecast_date", MySqlDbType.Date);
                forecastDateParam.Direction = System.Data.ParameterDirection.Input;
                forecastDateParam.Value = forecastDate;
                command.Parameters.Add(forecastDateParam);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        weatherPoint = new WeatherPoint()
                        {
                            CityName = reader.GetString("name"),
                            Path = reader.GetString("path"),
                            ForecastDate = reader.GetDateTime("forecast_date"),
                            ParsedAt = reader.GetDateTime("parsed_at"),
                            MinTempCelsius = reader.GetInt16("min_temp_celsius"),
                            MinTempFahrenheit = reader.GetInt16("min_temp_fahrenheit"),
                            MaxTempCelsius = reader.GetInt16("max_temp_celsius"),
                            MaxTempFahrenheit = reader.GetInt16("max_temp_fahrenheit"),
                            MaxWindSpeedMetersPerSecond = reader.GetUInt16("max_wind_speed_mps"),
                            MaxWindSpeedMilesPerHour = reader.GetUInt16("max_wind_speed_mph"),
                            MaxWindSpeedKilometersPerHour = reader.GetUInt16("max_wind_speed_kph"),
                            Summary = reader.GetString("summary"),
                        };
                        if (!reader.IsDBNull(11))
                        {
                            weatherPoint.PrecipitationAmount = reader.GetFloat("precipitation_amount");
                        }
                    }
                }
            }

            return weatherPoint;
        }

        public int SaveCity(string name, string path)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
            }

            using (var insertCommand = new MySqlCommand("INSERT INTO city (name, path) VALUES (@name, @path)", Connection))
            {
                var nameParam = new MySqlParameter("@name", name);
                insertCommand.Parameters.Add(nameParam);

                var pathParam = new MySqlParameter("@path", path);
                insertCommand.Parameters.Add(pathParam);

                var insertCount = insertCommand.ExecuteNonQuery();

                if (insertCount > 0)
                {
                    using (var command = new MySqlCommand("SELECT LAST_INSERT_ID();", Connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();

                            return reader.GetInt32(0);
                        }
                    }
                }
                return insertCount;
            }
        }

        public int SaveWeatherPoint(WeatherPoint weatherPoint, int cityId)
        {
            if (weatherPoint is null)
            {
                throw new ArgumentNullException(nameof(weatherPoint));
            }

            var query =
            @"INSERT INTO weather_point 
              (
                city_id,
                forecast_date,
                parsed_at,
                min_temp_celsius,
                min_temp_fahrenheit,
                max_temp_celsius,
                max_temp_fahrenheit,
                max_wind_speed_mps,
                max_wind_speed_mph,
                max_wind_speed_kph,
                precipitation_amount,
                summary
              ) VALUES (
                @city_id,
                @forecast_date,
                @parsed_at,
                @min_temp_celsius,
                @min_temp_fahrenheit,
                @max_temp_celsius,
                @max_temp_fahrenheit,
                @max_wind_speed_mps,
                @max_wind_speed_mph,
                @max_wind_speed_kph,
                @precipitation_amount,
                @summary
              )";

            using (var insertCommand = new MySqlCommand(query, Connection))
            {
                var cityIdParam = new MySqlParameter("@city_id", cityId);
                insertCommand.Parameters.Add(cityIdParam);

                var forecastDateParam = new MySqlParameter("@forecast_date", weatherPoint.ForecastDate);
                insertCommand.Parameters.Add(forecastDateParam);

                var parsedAtParam = new MySqlParameter("@parsed_at", weatherPoint.ParsedAt);
                insertCommand.Parameters.Add(parsedAtParam);

                var minTempCelsiusParam = new MySqlParameter("@min_temp_celsius", weatherPoint.MinTempCelsius);
                insertCommand.Parameters.Add(minTempCelsiusParam);

                var minTempFahrenheitParam = 
                    new MySqlParameter("@min_temp_fahrenheit", weatherPoint.MinTempFahrenheit);
                insertCommand.Parameters.Add(minTempFahrenheitParam);

                var maxTempCelsiusParam = new MySqlParameter("@max_temp_celsius", weatherPoint.MaxTempCelsius);
                insertCommand.Parameters.Add(maxTempCelsiusParam);

                var maxTempFahrenheitParam = 
                    new MySqlParameter("@max_temp_fahrenheit", weatherPoint.MaxTempFahrenheit);
                insertCommand.Parameters.Add(maxTempFahrenheitParam);

                var maxWindSpeedMetersPerSecondParam = 
                    new MySqlParameter("@max_wind_speed_mps", weatherPoint.MaxWindSpeedMetersPerSecond);
                insertCommand.Parameters.Add(maxWindSpeedMetersPerSecondParam);

                var maxWindSpeedMilesPerHourParam = 
                    new MySqlParameter("@max_wind_speed_mph", weatherPoint.MaxWindSpeedMilesPerHour);
                insertCommand.Parameters.Add(maxWindSpeedMilesPerHourParam);

                var maxWindSpeedKilometersPerHourParam = 
                    new MySqlParameter("@max_wind_speed_kph", weatherPoint.MaxWindSpeedKilometersPerHour);
                insertCommand.Parameters.Add(maxWindSpeedKilometersPerHourParam);

                var precipitationAmountParam = 
                    new MySqlParameter("@precipitation_amount", weatherPoint.PrecipitationAmount);
                insertCommand.Parameters.Add(precipitationAmountParam);

                var summaryParam = new MySqlParameter("@summary", weatherPoint.Summary);
                insertCommand.Parameters.Add(summaryParam);

                var insertCount = insertCommand.ExecuteNonQuery();

                if (insertCount > 0)
                {
                    using (var command = new MySqlCommand("SELECT LAST_INSERT_ID();", Connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();

                            return reader.GetInt32(0);
                        }
                    }
                }
                return insertCount;
            }
        }

        #region Init Database

        public void InitDatabase(
            string password,
            string userId,
            string databaseName,
            string server)
        {
            var checkIfDatabaseExistsQuery =
            $@"SELECT SCHEMA_NAME
               FROM INFORMATION_SCHEMA.SCHEMATA
               WHERE SCHEMA_NAME = '{databaseName}'";
            var createDatabaseQuery = $"CREATE DATABASE IF NOT EXISTS `{databaseName}` DEFAULT CHARSET cp1251;";

            var connectionString =
                $"Server={server};User ID={userId};Password={password}";

            using (var aConnection = new MySqlConnection(connectionString))
            {
                aConnection.Open();

                var checkIfDatabaseExistsCommand = new MySqlCommand(checkIfDatabaseExistsQuery, aConnection);

                if (checkIfDatabaseExistsCommand.ExecuteScalar() == null)
                {
                    var createDatabaseCommand = new MySqlCommand(createDatabaseQuery, aConnection);
                    createDatabaseCommand.ExecuteNonQuery();

                    CreateCityTable();
                    CreateWeatherPointTable();
                }
            }
        }

        private void CreateCityTable()
        {
            var query =
            @"CREATE TABLE IF NOT EXISTS `city` (
                `city_id` int unsigned NOT NULL AUTO_INCREMENT,
                `name` varchar(50) NOT NULL,
                `path` varchar(255) NOT NULL,
                PRIMARY KEY (`city_id`),
                UNIQUE KEY `path_UNIQUE` (`path`)
              ) ENGINE=InnoDB DEFAULT CHARSET=cp1251;";

            var command = new MySqlCommand(query, Connection);
            command.ExecuteNonQuery();
        }

        private void CreateWeatherPointTable()
        {
            var query =
            @"CREATE TABLE IF NOT EXISTS `weather_point` (
                `weather_point_id` int unsigned NOT NULL AUTO_INCREMENT,
                `city_id` int unsigned NOT NULL,
                `forecast_date` date NOT NULL,
                `parsed_at` datetime NOT NULL,
                `min_temp_celsius` smallint NOT NULL,
                `min_temp_fahrenheit` smallint NOT NULL,
                `max_temp_celsius` smallint NOT NULL,
                `max_temp_fahrenheit` smallint NOT NULL,
                `max_wind_speed_mps` smallint unsigned NOT NULL,
                `max_wind_speed_mph` smallint unsigned NOT NULL,
                `max_wind_speed_kph` smallint unsigned NOT NULL,
                `precipitation_amount` real unsigned DEFAULT NULL,
                `summary` varchar(255) DEFAULT NULL,
                PRIMARY KEY (`weather_point_id`),
                KEY `weather_point_city_id_city_city_id_idx` (`city_id`),
                CONSTRAINT `weather_point_city_id_city_city_id` FOREIGN KEY (`city_id`) REFERENCES `city` (`city_id`) ON DELETE CASCADE ON UPDATE CASCADE
              ) ENGINE=InnoDB DEFAULT CHARSET=cp1251;";

            var command = new MySqlCommand(query, Connection);
            command.ExecuteNonQuery();
        }

        #endregion
    }
}
