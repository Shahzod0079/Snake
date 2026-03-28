using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Snake
{
    public class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int localPort = 5001;
        public static int MaxSpeed = 15;

        static void Main(string[] args)
        {
            try
            {
                LoadLeaders();

                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                Thread tTime = new Thread(new ThreadStart(Timer));
                tTime.Start();

                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }
        }

        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                    IPAddress.Parse(User.IPAddress),
                    int.Parse(User.Port));
                try
                {
                    ViewModelGames vm = viewModelGames.Find(x => x.IdSnake == User.IdSnake);
                    if (vm != null)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vm));
                        sender.Send(bytes, bytes.Length, endPoint);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }
        }

        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(localPort);
            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                Console.WriteLine("Команды сервера: ");

                while (true)
                {
                    byte[] receiveBites = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBites);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду: " + returnData);

                    // Проверка на команду /start
                    if (returnData.StartsWith("/start"))
                    {
                        string[] dataMessage = returnData.Split('|');
                        if (dataMessage.Length >= 2)
                        {
                            ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress} : {viewModelUserSettings.Port}");
                            viewModelUserSettings.IdSnake = AddSnake();

                            // Проверка что индекс существует
                            if (viewModelUserSettings.IdSnake >= 0 && viewModelUserSettings.IdSnake < viewModelGames.Count)
                            {
                                viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                            }
                            remoteIPAddress.Add(viewModelUserSettings);
                        }
                    }
                    else
                    {
                        // Это не команда старт, а данные управления
                        try
                        {
                            string[] dataMessage = returnData.Split('|');
                            if (dataMessage.Length >= 2)
                            {
                                ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);

                                int IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress
                                    && x.Port == viewModelUserSettings.Port);

                                if (IdPlayer != -1 && viewModelGames.Count > IdPlayer && viewModelGames[IdPlayer].SnakesPlayers != null)
                                {
                                    if (dataMessage[0] == "Up" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                                        viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;
                                    else if (dataMessage[0] == "Down" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up)
                                        viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;
                                    else if (dataMessage[0] == "Left" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                                        viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                                    else if (dataMessage[0] == "Right" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                                        viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка обработки команды: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }
        }

        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();
            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                Points = new List<Snakes.Point>()
        {
            new Snakes.Point() { X = 30, Y = 10 },
            new Snakes.Point() { X = 20, Y = 10 },
            new Snakes.Point() { X = 10, Y = 10 }
        },
                direction = Snakes.Direction.Start
            };
            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.Count - 1; 
        }

        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);

                List<ViewModelGames> RemoteSnakes = viewModelGames.FindAll(x => x.SnakesPlayers != null && x.SnakesPlayers.GameOver);

                if (RemoteSnakes.Count > 0)
                {
                    foreach (ViewModelGames DeadSnake in RemoteSnakes)
                    {
                        var user = remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake);
                        if (user != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Отключил пользователя: {user.IPAddress}:{user.Port}");
                            remoteIPAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);
                        }
                    }
                }

                foreach (ViewModelUserSettings User in remoteIPAddress)
                {
                    if (User == null) continue;

                    var vm = viewModelGames.Find(x => x.IdSnake == User.IdSnake);
                    if (vm?.SnakesPlayers == null) continue;

                    Snakes Snake = vm.SnakesPlayers;
                    int Speed = 10 + (int)Math.Round(Snake.Points.Count / 20f);
                    if (Speed > MaxSpeed) Speed = MaxSpeed;

                    // Перемещение змеи
                    for (int i = Snake.Points.Count - 1; i > 0; i--)
                    {
                        Snake.Points[i] = new Snakes.Point()
                        {
                            X = Snake.Points[i - 1].X,
                            Y = Snake.Points[i - 1].Y
                        };
                    }

                    // Движение головы
                    if (Snake.direction == Snakes.Direction.Right)
                    {
                        Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X + Speed, Y = Snake.Points[0].Y };
                    }
                    if (Snake.direction == Snakes.Direction.Down)
                    {
                        Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X, Y = Snake.Points[0].Y + Speed };
                    }
                    if (Snake.direction == Snakes.Direction.Up)
                    {
                        Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X, Y = Snake.Points[0].Y - Speed };
                    }
                    if (Snake.direction == Snakes.Direction.Left)
                    {
                        Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X - Speed, Y = Snake.Points[0].Y };
                    }

                    // Проверка столкновения со стенами
                    if (Snake.Points[0].X <= 0 || Snake.Points[0].X >= 780 ||
                        Snake.Points[0].Y <= 0 || Snake.Points[0].Y >= 580)
                    {
                        Snake.GameOver = true;
                    }

                    // Проверка столкновения с собой
                    if (Snake.direction != Snakes.Direction.Start)
                    {
                        for (int i = 1; i < Snake.Points.Count; i++)
                        {
                            if (Math.Abs(Snake.Points[0].X - Snake.Points[i].X) <= 1 &&
                                Math.Abs(Snake.Points[0].Y - Snake.Points[i].Y) <= 1)
                            {
                                Snake.GameOver = true;
                                break;
                            }
                        }
                    }

                    // Проверка съедания яблока
                    var apple = viewModelGames.Find(x => x.IdSnake == User.IdSnake);
                    if (apple != null && Math.Abs(Snake.Points[0].X - apple.Points.X) <= 15 &&
                        Math.Abs(Snake.Points[0].Y - apple.Points.Y) <= 15)
                    {
                        apple.Points = new Snakes.Point()
                        {
                            X = new Random().Next(10, 783),
                            Y = new Random().Next(10, 410)
                        };

                        Snake.Points.Add(new Snakes.Point()
                        {
                            X = Snake.Points[Snake.Points.Count - 1].X,
                            Y = Snake.Points[Snake.Points.Count - 1].Y
                        });

                        LoadLeaders();

                        int currentPoints = Snake.Points.Count - 3; // ← сохраняем в переменную

                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = currentPoints
                        });

                        Leaders = Leaders.OrderByDescending(x => x.Points).ToList();

                        apple.Top = Leaders.FindIndex(x => x.Points == currentPoints && x.Name == User.Name) + 1;
                    }

                    if (Snake.GameOver)
                    {
                        LoadLeaders();

                        int finalPoints = Snake.Points.Count - 3; // ← сохраняем в переменную

                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = finalPoints
                        });

                        Leaders = Leaders.OrderByDescending(x => x.Points).ToList();
                        SaveLeaders();
                    }
                }
                Send();
            }
        }

        public static void SaveLeaders()
        {
            string json = JsonConvert.SerializeObject(Leaders);
            File.WriteAllText("./leaders.txt", json);
        }

        public static void LoadLeaders()
        {
            if (File.Exists("./leaders.txt"))
            {
                string json = File.ReadAllText("./leaders.txt");
                if (!string.IsNullOrEmpty(json))
                {
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                }
            }
            if (Leaders == null) Leaders = new List<Leaders>();
        }
    }
}