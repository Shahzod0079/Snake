using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using Common;

namespace SnakeWPF
{
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public ViewModelUserSettings viewModelUserSettings = new ViewModelUserSettings();
        public ViewModelGames ViewModelGames = null;
        public static IPAddress remoteIPAddress = IPAddress.Parse("127.0.0.1");
        public static int remotePort = 5001;
        public Thread tRec;
        public UdpClient receivingUdpClient;
        public Pages.Home Home = new Pages.Home();
        public Pages.Game Game = new Pages.Game();
        private object returndata;

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            OpenPage(Home);
        }

        public void StartReceiver()
        {
            // Останавливаем старый поток если есть
            if (tRec != null && tRec.IsAlive)
            {
                try { tRec.Abort(); } catch { }
                tRec = null;
            }

            tRec = new Thread(new ThreadStart(Receiver));
            tRec.Start();
        }

        public void OpenPage(Page page)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"Получены данные: {returndata}");
                DoubleAnimation startAnimation = new DoubleAnimation();
                startAnimation.From = 1;
                startAnimation.To = 0;
                startAnimation.Duration = TimeSpan.FromSeconds(0.6);
                startAnimation.Completed += delegate
                {
                    frame.Navigate(page);
                    DoubleAnimation endAnimation = new DoubleAnimation();
                    endAnimation.From = 0;
                    endAnimation.To = 1;
                    endAnimation.Duration = TimeSpan.FromSeconds(0.6);
                    frame.BeginAnimation(UIElement.OpacityProperty, endAnimation);
                };
                frame.BeginAnimation(UIElement.OpacityProperty, startAnimation);
            });
        }

        public void Receiver()
        {
            try
            {
                receivingUdpClient = new UdpClient(int.Parse(viewModelUserSettings.Port));
                IPEndPoint RemoteIpEndPoint = null;

                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returndata = Encoding.UTF8.GetString(receiveBytes);

                    if (returndata != null)
                    {
                        ViewModelGames = JsonConvert.DeserializeObject<ViewModelGames>(returndata);

                        Dispatcher.Invoke(() =>
                        {
                            if (ViewModelGames != null && ViewModelGames.SnakesPlayers != null && ViewModelGames.SnakesPlayers.GameOver)
                            {
                                OpenPage(new Pages.EndGame());
                            }
                            else if (ViewModelGames != null)
                            {
                                if (frame.Content != Game)
                                    OpenPage(Game);
                                Game.CreateUI();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }
        }

        public static void Send(string datagram)
        {
            UdpClient sender = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(datagram);
                sender.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        public void EventKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(viewModelUserSettings.IPAddress) &&
                !string.IsNullOrEmpty(viewModelUserSettings.Port) &&
                (ViewModelGames != null && ViewModelGames.SnakesPlayers != null && !ViewModelGames.SnakesPlayers.GameOver))
            {
                if (e.Key == System.Windows.Input.Key.Up)
                    Send($"Up|{JsonConvert.SerializeObject(viewModelUserSettings)}");
                else if (e.Key == System.Windows.Input.Key.Down)
                    Send($"Down|{JsonConvert.SerializeObject(viewModelUserSettings)}");
                else if (e.Key == System.Windows.Input.Key.Left)
                    Send($"Left|{JsonConvert.SerializeObject(viewModelUserSettings)}");
                else if (e.Key == System.Windows.Input.Key.Right)
                    Send($"Right|{JsonConvert.SerializeObject(viewModelUserSettings)}");
            }
        }

        private void QuitApplication(object sender, System.ComponentModel.CancelEventArgs e)
        {
            receivingUdpClient?.Close();
            tRec?.Abort();
        }
    }
}