using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace GismeteoParser
{
    public class Parser
    {
        const string GISMETEO_SITE_URL = "https://www.gismeteo.ru/";
        const string GISMETEO_SITE_TEN_DAYS_PATH = "10-days/";

        public Parser()
        { }

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
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(GISMETEO_SITE_URL);

            var aTagNodes = htmlDocument.DocumentNode.SelectNodes(
                "//html/body/section[2]/div[2]/div/div[1]/div/noscript/a");

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
                var parseUrl = GISMETEO_SITE_URL.TrimEnd('/') + path + GISMETEO_SITE_TEN_DAYS_PATH;
                var htmlWeb = new HtmlWeb();
                var htmlDocument = htmlWeb.Load(parseUrl);

                for (var i = 0; i < 10; i++)
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
            //var temperaturesDivNodes = htmlDocument.DocumentNode.SelectNodes(
            //    "/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div/div/div[1]/div/div[3]/div/div/div/div");
            var temperaturesDivNodes = htmlDocument.DocumentNode.SelectNodes(
                "//div[@class = 'templine w_temperature']/div/div/div");

            for (int i = 0; i < 10; i++)
            {
                var maxTempCelsiusSpanNode = temperaturesDivNodes[i].SelectSingleNode("div[1]/span[1]");
                cityWeatherPoints[i].MaxTempCelsius = 
                    sbyte.Parse(Parser.HtmlDecode(maxTempCelsiusSpanNode.InnerText));

                var maxTempFahrenheitSpanNode = temperaturesDivNodes[i].SelectSingleNode("div[1]/span[2]");
                cityWeatherPoints[i].MaxTempFahrenheit = 
                    sbyte.Parse(Parser.HtmlDecode(maxTempFahrenheitSpanNode.InnerText));

                var minTempCelsiusSpanNode = temperaturesDivNodes[i].SelectSingleNode("div[2]/span[1]");
                cityWeatherPoints[i].MinTempCelsius = 
                    sbyte.Parse(Parser.HtmlDecode(minTempCelsiusSpanNode.InnerText));

                var minTempFahrenheitSpanNode = temperaturesDivNodes[i].SelectSingleNode("div[2]/span[2]");
                cityWeatherPoints[i].MinTempFahrenheit = 
                    sbyte.Parse(Parser.HtmlDecode(minTempFahrenheitSpanNode.InnerText));
            }
        }

        private void ParseWindSpeeds(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var windSpeedsDivNodes = htmlDocument.DocumentNode.SelectNodes(
                "/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div/div/div[1]/div/div[5]/div");

            for (int i = 0; i < 10; i++)
            {
                var maxWindSpeedMetersPerSecondSpanNode = windSpeedsDivNodes[i].SelectSingleNode("div/div/span[1]");
                cityWeatherPoints[i].MaxWindSpeedMetersPerSecond = 
                    ushort.Parse(Parser.HtmlDecode(maxWindSpeedMetersPerSecondSpanNode.InnerText));

                var maxWindSpeedMilesPerHourSpanNode = windSpeedsDivNodes[i].SelectSingleNode("div/div/span[2]");
                cityWeatherPoints[i].MaxWindSpeedMilesPerHour = 
                    ushort.Parse(Parser.HtmlDecode(maxWindSpeedMilesPerHourSpanNode.InnerText));

                var maxWindSpeedKilometersPerHourSpanNode = windSpeedsDivNodes[i].SelectSingleNode("div/div/span[3]");
                cityWeatherPoints[i].MaxWindSpeedKilometersPerHour = 
                    ushort.Parse(Parser.HtmlDecode(maxWindSpeedKilometersPerHourSpanNode.InnerText));
            }
        }

        private void ParsePrecipitationAmounts(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var precipitationAmountDivNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[@class = 'widget__row widget__row_table widget__row_precipitation']");

            for (int i = 0; i < 10; i++)
            {
                if (precipitationAmountDivNode.SelectSingleNode("div[@class = 'w_prec__without']") != null)
                {
                    cityWeatherPoints[i].PrecipitationAmount = null;
                }
                else
                {
                    var precipitationAmountValueDivNode =
                        precipitationAmountDivNode.SelectSingleNode($"div[{i + 1}]/div/div[@class = 'w_prec__value']");
                    cityWeatherPoints[i].PrecipitationAmount =
                        float.Parse(Parser.HtmlDecode(precipitationAmountValueDivNode.InnerText.Replace(',', '.')));
                }
            }
        }

        private void ParseSummary(HtmlDocument htmlDocument, List<WeatherPoint> cityWeatherPoints)
        {
            var summaryDivNodes = htmlDocument.DocumentNode.SelectNodes(
                "/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div/div/div[1]/div/div[2]/div");

            for (int i = 0; i < 10; i++)
            {
                var summarySpanNode = summaryDivNodes[i].SelectSingleNode("div/span");
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
