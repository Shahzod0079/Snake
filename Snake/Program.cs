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
                    // Находим данные текущего игрока
                    ViewModelGames currentPlayer = viewModelGames.Find(x => x.IdSnake == User.IdSnake);

                    if (currentPlayer != null)
                    {
                        // Создаем копию для отправки
                        ViewModelGames dataToSend = new ViewModelGames()
                        {
                            IdSnake = currentPlayer.IdSnake,
                            Points = currentPlayer.Points,
                            Top = currentPlayer.Top,
                            SnakesPlayers = currentPlayer.SnakesPlayers
                        };

                        // Добавляем ВСЕХ змей (для отображения других игроков)
                        dataToSend.AllSnakes = new List<Snakes>();
                        foreach (var vm in viewModelGames)
                        {
                            if (vm.IdSnake != currentPlayer.IdSnake) // не добавляем свою змею (она уже в SnakesPlayers)
                            {
                                dataToSend.AllSnakes.Add(vm.SnakesPlayers);
                            }
                        }

                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataToSend));
                        sender.Send(bytes, bytes.Length, endPoint);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port} (всего змей: {dataToSend.AllSnakes.Count + 1})");
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

                    if (returnData.StartsWith("/start"))
                    {
                        string[] dataMessage = returnData.Split('|');
                        if (dataMessage.Length >= 2)
                        {
                            ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);

                            // ===== УДАЛЯЕМ СТАРОГО ИГРОКА С ТАКИМ ЖЕ IP И ПОРТОМ =====
                            var existingPlayer = remoteIPAddress.Find(x => x.IPAddress == viewModelUserSettings.IPAddress
                                                                            && x.Port == viewModelUserSettings.Port);
                            if (existingPlayer != null)
                            {
                                // Удаляем змею игрока
                                int index = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress
                                                                            && x.Port == viewModelUserSettings.Port);
                                if (index >= 0 && index < viewModelGames.Count)
                                {
                                    viewModelGames.RemoveAt(index);
                                }
                                remoteIPAddress.RemoveAll(x => x.IPAddress == viewModelUserSettings.IPAddress
                                                                && x.Port == viewModelUserSettings.Port);
                                Console.WriteLine($"Удалил старого игрока: {viewModelUserSettings.IPAddress}:{viewModelUserSettings.Port}");
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress} : {viewModelUserSettings.Port}");

                            viewModelUserSettings.IdSnake = AddSnake();

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
            new Snakes.Point() { X = 400, Y = 300 },  // голова
            new Snakes.Point() { X = 390, Y = 300 },  // тело
            new Snakes.Point() { X = 380, Y = 300 }   // хвост
        },
                direction = Snakes.Direction.Right  // ← меняем Start на Right
            };
            viewModelGamesPlayer.AllSnakes = new List<Snakes>();
            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(100, 700), new Random().Next(100, 500));
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
                    if (Snake.Points[0].X <= 20 || Snake.Points[0].X >= 780 ||
                        Snake.Points[0].Y <= 20 || Snake.Points[0].Y >= 580)
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