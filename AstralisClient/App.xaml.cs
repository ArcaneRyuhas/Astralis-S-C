using System;
using System.Diagnostics;
using System.Reflection;
using System.Media;
using System.Windows;
using System.IO;

namespace Astralis
{

    public partial class App : Application
    {
        public static void StartMusic()
        {
            string baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string relativePath = Path.Combine(baseDirectory, "Music", "ritual.wav");
            
            SoundPlayer player = new SoundPlayer(relativePath);
            player.PlayLooping();
        }

        public static void ChangeLenguage(string language)
        {
            switch (language){
                case "en":
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                    break;
                case "es-MX":
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-MX");
                    break;
            }
        }

        public static void RestartApplication()
        {
            string executablePath = Assembly.GetEntryAssembly().Location;

            Process.Start(executablePath);

            Environment.Exit(0);
        }
    }


}


