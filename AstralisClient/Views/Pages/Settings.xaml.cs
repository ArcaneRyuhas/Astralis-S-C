using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Astralis;

namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            ProgressBarValueChanged();
        }

        private void BtnSpanishClick(object sender, RoutedEventArgs e)
        {
            App.ChangeLenguage("es-MX");
            RestartWindow();
        }

        private void BtnEnglishClick(object sender, RoutedEventArgs e)
        {
            App.ChangeLenguage("en");
            RestartWindow();
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void RestartWindow()
        {
            Window currentWindow = Window.GetWindow(this);
            GameWindow gameWindow = new GameWindow();
            gameWindow.Show();

            currentWindow.Close();
        }

        private void ProgressBarValueChanged()
        {
            progessBarVolume.Value = 30;
        }
    }
}
