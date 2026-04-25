using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SnakeWPF.Pages
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void StartGame(object sender, EventArgs e)
        {
            // Сброс перед новой игрой
            MainWindow.mainWindow.ResetForNewGame();
            Thread.Sleep(50);

            // проверяем IP
            IPAddress UserIPAddress;
            if (!IPAddress.TryParse(ip.Text, out UserIPAddress))
            {
                MessageBox.Show("Please use the Ip address in the format X.X.X.X");
                return;
            }

            // проверяем порта
            int UserPort;
            if (!int.TryParse(port.Text, out UserPort))
            {
                MessageBox.Show("Please use the port as a number.");
                return;
            }

            MainWindow.mainWindow.viewModelUserSettings.IPAddress = ip.Text;
            MainWindow.mainWindow.viewModelUserSettings.Port = port.Text;
            MainWindow.mainWindow.viewModelUserSettings.Name = name.Text;

            MainWindow.mainWindow.StartReceiver();

            MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.viewModelUserSettings));

            MainWindow.mainWindow.OpenPage(MainWindow.mainWindow.Game);
        }
    }
}