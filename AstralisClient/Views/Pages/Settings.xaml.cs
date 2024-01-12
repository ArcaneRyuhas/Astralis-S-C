using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Astralis.Views.Pages
{
    public partial class Settings : Page
    {
        private const int MAX_VOLUME = 100;

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
            progessBarVolume.Value = MAX_VOLUME;
        }
    }
}
