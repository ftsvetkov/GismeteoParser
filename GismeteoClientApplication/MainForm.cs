using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GismeteoClientApplication
{
    public partial class MainForm : Form
    {
        private readonly GismeteoWeatherForecast.WeatherForecast weatherForecastService;

        public MainForm()
        {
            InitializeComponent();

            this.weatherForecastService = new GismeteoWeatherForecast.WeatherForecast();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var cities = weatherForecastService.GetCities();

            var citiesDatasource = new SortedDictionary<string, string>();
            citiesDatasource.Add("0", "Select city");
            foreach (var city in cities)
            {
                citiesDatasource.Add(city.Id.Value.ToString(), city.Name);
            }

            citiesComboBox.DataSource = new BindingSource(citiesDatasource, null);
            citiesComboBox.DisplayMember = "Value";
            citiesComboBox.ValueMember = "Key";

            citiesComboBox.SelectedValue = "0";

            dateTimePicker.Enabled = false;
            getForecastButton.Enabled = false;
        }

        private void citiesComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (citiesComboBox.SelectedIndex != 0)
            {
                int cityId = Convert.ToInt32(citiesComboBox.SelectedValue);
                var dates = weatherForecastService.GetAvailableForecastDates(cityId);

                dateTimePicker.MinDate = dates.Min();
                dateTimePicker.MaxDate = dates.Max();

                dateTimePicker.Enabled = true;
                getForecastButton.Enabled = true;
            }
            else
            {
                dateTimePicker.Enabled = false;
                getForecastButton.Enabled = false;
            }
        }

        private void getForecastButton_Click(object sender, EventArgs e)
        {
            int cityId = Convert.ToInt32(citiesComboBox.SelectedValue);
            var date = dateTimePicker.Value;

            var weatherPoint = this.weatherForecastService.GetCityWeatherForecastOnDate(cityId, date);

            var sb = new StringBuilder();
            sb.AppendFormat("City: {0}", weatherPoint.CityName);
            sb.AppendLine();
            sb.AppendFormat("Path: {0}", weatherPoint.Path);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Forecast date: {0}", weatherPoint.ForecastDate.ToString("yyyy/MM/dd"));
            sb.AppendLine();
            sb.AppendFormat("Parsed at: {0}", weatherPoint.ParsedAt.ToString("yyyy/MM/dd HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Minimum temperature in Celsius degree: {0}", weatherPoint.MinTempCelsius);
            sb.AppendLine();
            sb.AppendFormat("Minimum temperature in Fahrenheit degree: {0}", weatherPoint.MinTempFahrenheit);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Maximum temperature in Celsius degree: {0}", weatherPoint.MaxTempCelsius);
            sb.AppendLine();
            sb.AppendFormat("Maximum temperature in Fahrenheit degree: {0}", weatherPoint.MaxTempFahrenheit);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Maximum wind speed in meters per second: {0}", weatherPoint.MaxWindSpeedMetersPerSecond);
            sb.AppendLine();
            sb.AppendFormat("Maximum wind speed in miles per hour: {0}", weatherPoint.MaxWindSpeedMilesPerHour);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Maximum wind speed in kilometers per hour: {0}", weatherPoint.MaxWindSpeedKilometersPerHour);
            sb.AppendLine();
            sb.AppendFormat("Precipitation amount in millimeters: {0}", weatherPoint.PrecipitationAmount);
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("Summary:");
            sb.AppendLine();
            sb.Append(weatherPoint.Summary);

            forecastTextBox.Text = sb.ToString();
        }
    }
}
