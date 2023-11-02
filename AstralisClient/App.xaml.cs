using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Astralis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void RestartApplication()
        {
            string executablePath = Assembly.GetEntryAssembly().Location;

            Process.Start(executablePath);

            Environment.Exit(0);
        }
    }


}


