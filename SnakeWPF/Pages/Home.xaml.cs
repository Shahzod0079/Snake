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
            // Сбрасываем старые данные
            MainWindow.mainWindow.ViewModelGames = null;

            // Закрываем старый сокет и останавливаем поток (ВАЖНО!)
            try
            {
                MainWindow.mainWindow.receivingUdpClient?.Close();
                MainWindow.mainWindow.receivingUdpClient = null;
            }
            catch { }

            try
            {
                MainWindow.mainWindow.tRec?.Abort();
                MainWindow.mainWindow.tRec = null;
            }
            catch { }

            // Небольшая задержка, чтобы порт освободился
            System.Threading.Thread.Sleep(100);

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