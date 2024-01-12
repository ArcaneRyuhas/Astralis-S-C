using System;
using System.Configuration;
using System.ServiceModel;

namespace Host
{
    public static class Program
    {
        static void Main(string[] args)
        {
            GetConnectionString();

            using (ServiceHost host = new ServiceHost(typeof(MessageService.MessageService)))
            {
                host.Open();
                Console.WriteLine("Server is running");
                Console.ReadLine();
            }
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
        }
    }
}

