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

            // НЕ закрываем сокет и не прерываем поток здесь!
            // Просто сбрасываем данные игры
            MainWindow.mainWindow.ViewModelGames = null;
        }

        private void OpenHome(object sender, RoutedEventArgs e)
        {
            // Не закрываем сокет здесь! Это сделает StartGame при новом запуске
            MainWindow.mainWindow.ViewModelGames = null;
            MainWindow.mainWindow.Home = new Home();
            MainWindow.mainWindow.OpenPage(MainWindow.mainWindow.Home);
        }
    }
}