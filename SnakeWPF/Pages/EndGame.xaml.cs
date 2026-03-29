using System.Windows;
using System.Windows.Controls;
using Common;

namespace SnakeWPF.Pages
{
    public partial class EndGame : Page
    {
        public EndGame()
        {
            InitializeComponent();

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

            // Сбрасываем данные игры
            MainWindow.mainWindow.ViewModelGames = null;
        }

        private void OpenHome(object sender, RoutedEventArgs e)
        {
            // Очищаем старые данные перед новой игрой
            MainWindow.mainWindow.ViewModelGames = null;

            // Создаем новую страницу Home
            MainWindow.mainWindow.Home = new Home();

            // Переходим на Home
            MainWindow.mainWindow.OpenPage(MainWindow.mainWindow.Home);
        }
    }
}