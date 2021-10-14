using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace GismeteoParser
{
    public enum ParserMode
    {
        TenDays = 10,
        TwoWeeks = 14,
    }

    public class Parser
    {
        const string GISMETEO_SITE_URL = "https://www.gismeteo.ru/";
        const string GISMETEO_SITE_TEN_DAYS_PATH = "10-days/";
        const string GISMETEO_SITE_TWO_WEEKS_PATH = "2-weeks/";

        private readonly ParserMode mode;

        public Parser(ParserMode mode = ParserMode.TenDays)
        {
            this.mode = mode;
        }

        public Dictionary<string, List<WeatherPoint>> Parse()
        {
            var citiesEntryPoints = ParseCitiesEntryPoints();

            if (citiesEntryPoints != null)
            {
                var weatherForecast = new Dictionary<string, List<WeatherPoint>>();
                var parsedAt = DateTime.Now;

                foreach (var entryPoint in citiesEntryPoints)
                {
                    var cityWeatherPoints = ParseCityWeatherPoints(entryPoint.Key, entryPoint.Value, parsedAt);

                    if (cityWeatherPoints != null)
                    {
                        weatherForecast[entryPoint.Value] = cityWeatherPoints;
                    }
                }

                return weatherForecast;
            }
            return null;
        }

        public Dictionary<string, string> ParseCitiesEntryPoints()
        {
            // Во время написания программы в какой-то момент были проблемы
            // с получением данных с сайта, которые решилсь подключением старых протоколов.
            //ServicePointManager.SecurityProtocol |=
            //    SecurityProtocolType.Tls | 
            //    SecurityProtocolType.Tls11 | 
            //    SecurityProtocolType.Tls12 | 
            //    SecurityProtocolType.Ssl3;

            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(GISMETEO_SITE_URL);

            var aTagNodes = htmlDocument.DocumentNode.SelectNodes("//noscript[@id='noscript']/a");

            if (aTagNodes != null)
            {
                var citiesEntryPoints = new Dictionary<string, string>();

                foreach (var aTagNode in aTagNodes)
                {
                    var path = aTagNode.GetAttributeValue("href", "");
                    var name = aTagNode.GetAttributeValue("data-name", "");

                    if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(name))
                    {
                        citiesEntryPoints[name] = path;
                    }
                }

                return citiesEntryPoints;
            }
            return null;
        }

        public List<WeatherPoint> ParseCityWeatherPoints(string cityName, string path, DateTime parsedAt)
        {
            if (!String.IsNullOrEmpty(path))
            {
                var cityWeatherPoints = new List<WeatherPoint>();
                var parseUrl = GISMETEO_SITE_URL.TrimEnd('/') + path + 
                    (mode == ParserMode.TenDays ? GISMETEO_SITE_TEN_DAYS_PATH : GISMETEO_SITE_TWO_WEEKS_PATH);
                var htmlWeb = new HtmlWeb();
                var htmlDocument = htmlWeb.Load(parseUrl);

                var modeValue = (int)mode;
                for (var i = 0; i < modeValue; i++)
                {
                    cityWeatherPoints.Add(new WeatherPoint()
                    {
                        CityName = cityName,
                        Path = path,
                        ForecastDate = parsedAt.AddDays(i),
                        ParsedAt = parsedAt,
                    });
                }

                ParseTemperatures(htmlDocument, cityWeatherPoints);
                ParseWindSpeeds(htmlDocument, cityWeatherPoints);
                ParsePrecipitationAmounts(htmlDocument, cityWeatherPoints);
                ParseSummary(htmlDocument, cityWeatherPoints);

                return cityWeatherPoints;
            }
            return null;
        }

        private void ParseTemperatures(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var temperaturesDivNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[@class='templine w_temperature']/div[@class='chart chart__temperature']/div[@class='values']");

            var modeValue = (int)mode;
            for (var i = 0; i < modeValue; i++)
            {
                var maxTempCelsiusSpanNode = temperaturesDivNode.SelectSingleNode(
                    $"div[{i + 1}]/div[@class='maxt']/span[@class='unit unit_temperature_c']");
                cityWeatherPoints[i].MaxTempCelsius = 
                    sbyte.Parse(Parser.HtmlDecode(maxTempCelsiusSpanNode.InnerText));

                var maxTempFahrenheitSpanNode = temperaturesDivNode.SelectSingleNode(
                    $"div[{i + 1}]/div[@class='maxt']/span[@class='unit unit_temperature_f']");
                cityWeatherPoints[i].MaxTempFahrenheit = 
                    sbyte.Parse(Parser.HtmlDecode(maxTempFahrenheitSpanNode.InnerText));

                var minTempCelsiusSpanNode = temperaturesDivNode.SelectSingleNode(
                    $"div[{i + 1}]/div[@class='mint']/span[@class='unit unit_temperature_c']");
                cityWeatherPoints[i].MinTempCelsius = 
                    sbyte.Parse(Parser.HtmlDecode(minTempCelsiusSpanNode.InnerText));

                var minTempFahrenheitSpanNode = temperaturesDivNode.SelectSingleNode(
                    $"div[{i + 1}]/div[@class='mint']/span[@class='unit unit_temperature_f']");
                cityWeatherPoints[i].MinTempFahrenheit = 
                    sbyte.Parse(Parser.HtmlDecode(minTempFahrenheitSpanNode.InnerText));
            }
        }

        private void ParseWindSpeeds(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var windSpeedsDivNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[@class='widget__row widget__row_table widget__row_wind-or-gust']");

            var modeValue = (int)mode;
            for (var i = 0; i < modeValue; i++)
            {
                var maxWindSpeedMetersPerSecondSpanNode =
                    windSpeedsDivNode.SelectSingleNode($"div[@data-item='{i}']/div/div/span[@class='unit unit_wind_m_s']");
                cityWeatherPoints[i].MaxWindSpeedMetersPerSecond =
                    ushort.Parse(Parser.HtmlDecode(maxWindSpeedMetersPerSecondSpanNode.InnerText));

                var maxWindSpeedMilesPerHourSpanNode =
                    windSpeedsDivNode.SelectSingleNode($"div[@data-item='{i}']/div/div/span[@class='unit unit_wind_mi_h']");
                cityWeatherPoints[i].MaxWindSpeedMilesPerHour =
                    ushort.Parse(Parser.HtmlDecode(maxWindSpeedMilesPerHourSpanNode.InnerText));

                var maxWindSpeedKilometersPerHourSpanNode =
                    windSpeedsDivNode.SelectSingleNode($"div[@data-item='{i}']/div/div/span[@class='unit unit_wind_km_h']");
                cityWeatherPoints[i].MaxWindSpeedKilometersPerHour =
                    ushort.Parse(Parser.HtmlDecode(maxWindSpeedKilometersPerHourSpanNode.InnerText));
            }
        }

        private void ParsePrecipitationAmounts(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var precipitationAmountDivNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[@class='widget__row widget__row_table widget__row_precipitation']");

            var modeValue = (int)mode;
            for (var i = 0; i < modeValue; i++)
            {
                if (precipitationAmountDivNode.SelectSingleNode("div[@class='w_prec__without']") != null)
                {
                    cityWeatherPoints[i].PrecipitationAmount = null;
                }
                else
                {
                    var precipitationAmountValueDivNode =
                        precipitationAmountDivNode.SelectSingleNode($"div[{i + 1}]/div/div[@class='w_prec__value']");
                    cityWeatherPoints[i].PrecipitationAmount =
                        float.Parse(Parser.HtmlDecode(precipitationAmountValueDivNode.InnerText.Replace(',', '.')));
                }
            }
        }

        private void ParseSummary(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var summaryDivNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[@class='widget__row widget__row_table widget__row_icon']");

            var modeValue = (int)mode;
            for (var i = 0; i < modeValue; i++)
            {
                var summarySpanNode = 
                    summaryDivNode.SelectSingleNode($"div[@data-item='{i}']/div/span[@class='tooltip']");
                cityWeatherPoints[i].Summary =
                    Parser.HtmlDecode(summarySpanNode.GetAttributeValue("data-text", ""));
            }
        }

        private static string HtmlDecode(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return text;
            }
            return HttpUtility.HtmlDecode(text.Replace("&minus;", "-"));
        }
    }
}
