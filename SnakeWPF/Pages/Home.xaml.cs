using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            if (MainWindow.mainWindow.receivingUdpClient != null)
            {
                MainWindow.mainWindow.receivingUdpClient.Close();
                if (MainWindow.mainWindow.tRec != null)
                {
                    MainWindow.mainWindow.tRec.Abort();

                    IPAddress UserIPAddress;
                    if (!IPAddress.TryParse(ip.Text, out UserIPAddress))
                    {
                        MessageBox.Show("Please use the Ip address in the formst X.X.X.X ");
                        return;
                    }
                    int UserPort;
                    if (!int.TryParse(port.Text, out UserPort))
                    {
                        MessageBox.Show("Please use the port as a number.");
                        return;
                    }
                    MainWindow.mainWindow.StartReceiver();
                    MainWindow.mainWindow.viewModelUserSettings.IpAddress = ip.Text;
                    MainWindow.mainWindow.viewModelUserSettings.Port = port.Text;
                    MainWindow.mainWindow.viewModelUserSettings.Name = port.Name;
                    MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.viewModelUserSettings));

                }
            }
        }
    }
}
