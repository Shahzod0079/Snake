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

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            OpenPage(Home);
        }

        public void OpenPage(Page page)
        {
            Dispatcher.Invoke(() =>
            {
                var anim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.6));
                anim.Completed += (s, _) =>
                {
                    frame.Navigate(page);
                    frame.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6)));
                };
                frame.BeginAnimation(UIElement.OpacityProperty, anim);
            });
        }

        public void StartReceiver()
        {
            // Закрываем старый сокет, если есть
            try
            {
                receivingUdpClient?.Close();
                receivingUdpClient = null;
            }
            catch { }

            // Останавливаем старый поток, если есть
            try
            {
                tRec?.Abort();
                tRec = null;
            }
            catch { }

            // Создаём новый поток
            tRec = new Thread(Receiver);
            tRec.Start();
        }
        public void Receiver()
        {
            try
            {
                receivingUdpClient = new UdpClient(int.Parse(viewModelUserSettings.Port));
                IPEndPoint remoteEndPoint = null;
                while (true)
                {
                    byte[] bytes = receivingUdpClient.Receive(ref remoteEndPoint);
                    string data = Encoding.UTF8.GetString(bytes);
                    var vm = JsonConvert.DeserializeObject<ViewModelGames>(data);
                    if (vm == null) continue;

                    Dispatcher.Invoke(() =>
                    {
                        // Если игра уже закончилась или мы на странице EndGame - игнорируем новые данные
                        if (ViewModelGames?.SnakesPlayers?.GameOver == true || frame.Content is Pages.EndGame)
                            return;

                        if (vm.SnakesPlayers?.GameOver == true)
                        {
                            ViewModelGames = vm;
                            OpenPage(new Pages.EndGame());
                        }
                        else
                        {
                            ViewModelGames = vm;
                            if (frame.Content != Game) OpenPage(Game);
                            Game.CreateUI();
                        }
                    });
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"Receiver error: {ex.Message}");
            }
        }

        public static void Send(string datagram)
        {
            using (var sender = new UdpClient())
            {
                var endPoint = new IPEndPoint(remoteIPAddress, remotePort);
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(datagram);
                    sender.Send(bytes, bytes.Length, endPoint);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
        }

        public void EventKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModelUserSettings.IPAddress) || string.IsNullOrEmpty(viewModelUserSettings.Port)) return;
            if (ViewModelGames?.SnakesPlayers?.GameOver == true) return;

            string cmd = null;
            if (e.Key == System.Windows.Input.Key.Up) cmd = "Up";
            else if (e.Key == System.Windows.Input.Key.Down) cmd = "Down";
            else if (e.Key == System.Windows.Input.Key.Left) cmd = "Left";
            else if (e.Key == System.Windows.Input.Key.Right) cmd = "Right";
            if (cmd != null) Send($"{cmd}|{JsonConvert.SerializeObject(viewModelUserSettings)}");
        }

        private void QuitApplication(object sender, System.ComponentModel.CancelEventArgs e)
        {
            receivingUdpClient?.Close();
            tRec?.Abort();
        }
    }
}