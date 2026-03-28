using System.Windows;
using System.Windows.Controls;

namespace SnakeWPF.Pages
{
    public partial class EndGame : Page
    {
        public EndGame()
        {
            InitializeComponent();

            // Проверка на null
            if (MainWindow.mainWindow.viewModelUserSettings != null)
                name.Content = MainWindow.mainWindow.viewModelUserSettings.Name;

            if (MainWindow.mainWindow.ViewModelGames?.SnakesPlayers?.Points != null)
            {
                top.Content = MainWindow.mainWindow.ViewModelGames.Top;
                glasses.Content = $"{MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 3} glasses";
            }

            // Закрываем соединения
            try
            {
                MainWindow.mainWindow.receivingUdpClient?.Close();
                MainWindow.mainWindow.tRec?.Abort();
            }
            catch { }

            MainWindow.mainWindow.ViewModelGames = null;
        }

        private void OpenHome(object sender, RoutedEventArgs e)
        {
            // Переход на страницу Home
            MainWindow.mainWindow.frame.Navigate(MainWindow.mainWindow.Home);
        }
    }
}