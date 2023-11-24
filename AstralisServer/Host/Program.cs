using System;
using System.IO;
using System.ServiceModel;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("Log4Net.config"));

            using (ServiceHost host = new ServiceHost(typeof(MessageService.UserManager)))
            {

                host.Open();
                Console.WriteLine("Server is running");
                Console.ReadLine();
            }
        }
    }
}
