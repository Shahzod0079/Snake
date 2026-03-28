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
            // Проверка что receivingUdpClient не null
            if (MainWindow.mainWindow.receivingUdpClient != null)
            {
                MainWindow.mainWindow.receivingUdpClient.Close();

                if (MainWindow.mainWindow.tRec != null)
                {
                    MainWindow.mainWindow.tRec.Abort();
                }
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

            // Запуск получения данных
            MainWindow.mainWindow.StartReceiver();

            // Сохраняем настройки пользователя
            MainWindow.mainWindow.viewModelUserSettings.IPAddress = ip.Text;
            MainWindow.mainWindow.viewModelUserSettings.Port = port.Text;
            MainWindow.mainWindow.viewModelUserSettings.Name = name.Text;  // ← исправлено: было port.Name

            // Отправляем данные на сервер
            MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.viewModelUserSettings));
        }
    }
}