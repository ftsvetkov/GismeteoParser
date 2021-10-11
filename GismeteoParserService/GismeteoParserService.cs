using GismeteoParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GismeteoParserService
{
    public partial class GismeteoParserService : ServiceBase
    {
        private DatabaseProcessor databaseProcessor;
        private Timer timer;

        public GismeteoParserService()
        {
            InitializeComponent();

            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            this.databaseProcessor = new DatabaseProcessor(password: "DB_USER_PASSWORD");

            this.timer = new Timer();
            this.timer.Interval = 5 * 60 * 1000;
            this.timer.Enabled = true;
            this.timer.Elapsed += Timer_Tick;
        }

        protected override void OnPause()
        {
            this.timer.Enabled = false;
        }

        protected override void OnContinue()
        {
            this.timer.Enabled = true;
        }

        protected override void OnStop()
        {
            this.timer.Enabled = false;
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            var parser = new Parser();
            var weatherForecast = parser.Parse();

            this.databaseProcessor.SaveWeatherForecast(weatherForecast);
        }
    }
}
