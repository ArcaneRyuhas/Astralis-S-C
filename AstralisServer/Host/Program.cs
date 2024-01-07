using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;

namespace Host
{
    public static class Program
    {


        static void Main(string[] args)
        {
            // Lee la variable de entorno
            string stringConfiguration = Environment.GetEnvironmentVariable("StringConfiguration");

            // Obtiene la configuración de la cadena de conexión del archivo de configuración
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionString = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"].ConnectionString;

            // Reemplaza el marcador de posición con el valor de la variable de entorno
            connectionString = connectionString.Replace("%StringConfiguration%", stringConfiguration);

            // Actualiza la cadena de conexión en el archivo de configuración
            config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"].ConnectionString = connectionString;
            config.Save(ConfigurationSaveMode.Modified);

            // Vuelve a cargar la configuración
            ConfigurationManager.RefreshSection("connectionStrings");

            // Resto de la lógica de tu aplicación

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
