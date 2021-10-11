using GismeteoParser;
using System;
using System.Collections.Generic;
using System.Web.Services;

namespace GismeteoWebService
{
    /// <summary>
    /// Summary description for WeatherForecast
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WeatherForecast : System.Web.Services.WebService
    {
        public const string DATABASE_PASSWORD = "DB_USER_PASSWORD";

        [WebMethod]
        public List<City> GetCities()
        {
            var dp = new DatabaseProcessor(password: DATABASE_PASSWORD);
            return dp.GetCities();
        }

        [WebMethod]
        public List<DateTime> GetAvailableForecastDates(int cityId)
        {
            var dp = new DatabaseProcessor(password: DATABASE_PASSWORD);
            return dp.GetAvailableForecastDates(cityId);
        }

        [WebMethod]
        public WeatherPoint GetCityWeatherForecastOnDate(int cityId, DateTime date)
        {
            var dp = new DatabaseProcessor(password: DATABASE_PASSWORD);
            return dp.GetCityWeatherForecastOnDate(cityId, date);
        }
    }
}
