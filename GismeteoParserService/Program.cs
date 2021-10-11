using System.ServiceProcess;

namespace GismeteoParserService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new GismeteoParserService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
