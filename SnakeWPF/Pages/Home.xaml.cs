using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void StartGame(object sender, EventArgs e)
        {
            // Сбрасываем старые данные перед новой игрой
            MainWindow.mainWindow.ViewModelGames = null;

            // Очищаем старые соединения
            if (MainWindow.mainWindow.receivingUdpClient != null)
            {
                try { MainWindow.mainWindow.receivingUdpClient.Close(); } catch { }
                MainWindow.mainWindow.receivingUdpClient = null;
            }

            if (MainWindow.mainWindow.tRec != null)
            {
                try { MainWindow.mainWindow.tRec.Abort(); } catch { }
                MainWindow.mainWindow.tRec = null;
            }

            // Проверка IP адреса
            IPAddress UserIPAddress;
            if (!IPAddress.TryParse(ip.Text, out UserIPAddress))
            {
                MessageBox.Show("Please use the Ip address in the format X.X.X.X");
                return;
            }

            // Проверка порта
            int UserPort;
            if (!int.TryParse(port.Text, out UserPort))
            {
                MessageBox.Show("Please use the port as a number.");
                return;
            }

            // Сохраняем настройки пользователя
            MainWindow.mainWindow.viewModelUserSettings.IPAddress = ip.Text;
            MainWindow.mainWindow.viewModelUserSettings.Port = port.Text;
            MainWindow.mainWindow.viewModelUserSettings.Name = name.Text;

            // Запуск получения данных
            MainWindow.mainWindow.StartReceiver();

            // Отправляем данные на сервер
            MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.viewModelUserSettings));

            // Переход на страницу Game
            MainWindow.mainWindow.OpenPage(MainWindow.mainWindow.Game);
        }
    }
}