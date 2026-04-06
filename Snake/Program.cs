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
                Console.WriteLine(ex.ToString());
            }
        }

        private static void Send()
        {
            foreach (ViewModelUserSettings user in remoteIPAddress.ToList())
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(user.IPAddress), int.Parse(user.Port));
                try
                {
                    var current = viewModelGames.FirstOrDefault(x => x.IdSnake == user.IdSnake);
                    if (current == null) continue;

                    var dataToSend = new ViewModelGames
                    {
                        IdSnake = current.IdSnake,
                        Points = current.Points,
                        Top = current.Top,
                        SnakesPlayers = current.SnakesPlayers,
                        AllSnakes = viewModelGames.Where(x => x.IdSnake != current.IdSnake && x.SnakesPlayers?.GameOver == false)
                                                  .Select(x => x.SnakesPlayers).ToList()
                    };
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataToSend));
                    sender.Send(bytes, bytes.Length, endPoint);
                }
                catch (Exception ex) { Console.WriteLine("Send error: " + ex.Message); }
                finally { sender.Close(); }
            }
        }

        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(localPort);
            IPEndPoint remoteIpEndPoint = null;
            try
            {
                while (true)
                {
                    byte[] data = receivingUdpClient.Receive(ref remoteIpEndPoint);
                    string msg = Encoding.UTF8.GetString(data);
                    Console.WriteLine("Получено: " + msg);

                    if (msg.StartsWith("/start"))
                    {
                        var parts = msg.Split('|');
                        if (parts.Length >= 2)
                        {
                            var user = JsonConvert.DeserializeObject<ViewModelUserSettings>(parts[1]);

                            // Найти старого игрока с таким же IP и портом
                            var oldPlayer = remoteIPAddress.FirstOrDefault(x => x.IPAddress == user.IPAddress && x.Port == user.Port);
                            if (oldPlayer != null)
                            {
                                // Удалить старого игрока из списков
                                remoteIPAddress.RemoveAll(x => x.IPAddress == user.IPAddress && x.Port == user.Port);
                                viewModelGames.RemoveAll(x => x.IdSnake == oldPlayer.IdSnake);
                                Console.WriteLine($"Удалён старый игрок {oldPlayer.IPAddress}:{oldPlayer.Port} с IdSnake={oldPlayer.IdSnake}");
                            }

                            // Создать нового игрока
                            user.IdSnake = AddSnake();
                            viewModelGames[user.IdSnake].IdSnake = user.IdSnake;
                            remoteIPAddress.Add(user);
                            Console.WriteLine($"Подключился новый игрок {user.IPAddress}:{user.Port} -> IdSnake={user.IdSnake}");
                        }
                    }
                    else
                    {
                        var parts = msg.Split('|');
                        if (parts.Length >= 2)
                        {
                            var user = JsonConvert.DeserializeObject<ViewModelUserSettings>(parts[1]);
                            int idx = remoteIPAddress.FindIndex(x => x.IPAddress == user.IPAddress && x.Port == user.Port);
                            if (idx >= 0 && idx < viewModelGames.Count && viewModelGames[idx].SnakesPlayers != null && !viewModelGames[idx].SnakesPlayers.GameOver)
                            {
                                var dir = viewModelGames[idx].SnakesPlayers.direction;
                                switch (parts[0])
                                {
                                    case "Up": if (dir != Snakes.Direction.Down) viewModelGames[idx].SnakesPlayers.direction = Snakes.Direction.Up; break;
                                    case "Down": if (dir != Snakes.Direction.Up) viewModelGames[idx].SnakesPlayers.direction = Snakes.Direction.Down; break;
                                    case "Left": if (dir != Snakes.Direction.Right) viewModelGames[idx].SnakesPlayers.direction = Snakes.Direction.Left; break;
                                    case "Right": if (dir != Snakes.Direction.Left) viewModelGames[idx].SnakesPlayers.direction = Snakes.Direction.Right; break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Receiver error: " + ex.Message); }
        }

        public static int AddSnake()
        {
            var newSnake = new ViewModelGames
            {
                SnakesPlayers = new Snakes
                {
                    Points = new List<Snakes.Point>
                    {
                        new Snakes.Point(400, 300),
                        new Snakes.Point(390, 300),
                        new Snakes.Point(380, 300)
                    },
                    direction = Snakes.Direction.Right,
                    GameOver = false
                },
                Points = new Snakes.Point(new Random().Next(100, 700), new Random().Next(100, 500)),
                AllSnakes = new List<Snakes>()
            };
            viewModelGames.Add(newSnake);
            return viewModelGames.Count - 1;
        }

        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);
                foreach (var user in remoteIPAddress.ToList())
                {
                    var vm = viewModelGames.FirstOrDefault(x => x.IdSnake == user.IdSnake);
                    if (vm?.SnakesPlayers == null || vm.SnakesPlayers.GameOver) continue;

                    var snake = vm.SnakesPlayers;
                    int speed = 8 + snake.Points.Count / 20;
                    if (speed > MaxSpeed) speed = MaxSpeed;

                    // Двигаем тело
                    for (int i = snake.Points.Count - 1; i > 0; i--)
                        snake.Points[i] = new Snakes.Point(snake.Points[i - 1].X, snake.Points[i - 1].Y);

                    // Двигаем голову
                    switch (snake.direction)
                    {
                        case Snakes.Direction.Right: snake.Points[0] = new Snakes.Point(snake.Points[0].X + speed, snake.Points[0].Y); break;
                        case Snakes.Direction.Left: snake.Points[0] = new Snakes.Point(snake.Points[0].X - speed, snake.Points[0].Y); break;
                        case Snakes.Direction.Up: snake.Points[0] = new Snakes.Point(snake.Points[0].X, snake.Points[0].Y - speed); break;
                        case Snakes.Direction.Down: snake.Points[0] = new Snakes.Point(snake.Points[0].X, snake.Points[0].Y + speed); break;
                    }

                    // Столкновение со стенами
                    if (snake.Points[0].X <= 20 || snake.Points[0].X >= 780 || snake.Points[0].Y <= 20 || snake.Points[0].Y >= 580)
                    {
                        snake.GameOver = true;
                        Console.WriteLine($"{user.Name} врезался в стену!");
                    }

                    // Столкновение с собой
                    for (int i = 1; i < snake.Points.Count; i++)
                    {
                        if (Math.Abs(snake.Points[0].X - snake.Points[i].X) <= 1 && Math.Abs(snake.Points[0].Y - snake.Points[i].Y) <= 1)
                        {
                            snake.GameOver = true;
                            Console.WriteLine($"{user.Name} врезался в себя!");
                            break;
                        }
                    }

                    // Съедание яблока
                    if (Math.Abs(snake.Points[0].X - vm.Points.X) <= 15 && Math.Abs(snake.Points[0].Y - vm.Points.Y) <= 15)
                    {
                        vm.Points = new Snakes.Point(new Random().Next(100, 700), new Random().Next(100, 500));
                        snake.Points.Add(new Snakes.Point(snake.Points.Last().X, snake.Points.Last().Y));
                        // Обновляем рекорды
                        LoadLeaders();
                        Leaders.Add(new Leaders { Name = user.Name, Points = snake.Points.Count - 3 });
                        Leaders = Leaders.OrderByDescending(x => x.Points).ToList();
                        vm.Top = Leaders.FindIndex(x => x.Name == user.Name && x.Points == snake.Points.Count - 3) + 1;
                        SaveLeaders();
                    }

                    // Если игра окончена – сохраняем рекорд и НЕ удаляем игрока (чтобы клиент получил GameOver)
                    if (snake.GameOver)
                    {
                        LoadLeaders();
                        Leaders.Add(new Leaders { Name = user.Name, Points = snake.Points.Count - 3 });
                        Leaders = Leaders.OrderByDescending(x => x.Points).ToList();
                        SaveLeaders();
                    }
                }
                Send();
            }
        }

        public static void SaveLeaders() => File.WriteAllText("leaders.txt", JsonConvert.SerializeObject(Leaders));
        public static void LoadLeaders()
        {
            if (File.Exists("leaders.txt"))
            {
                string json = File.ReadAllText("leaders.txt");
                if (!string.IsNullOrEmpty(json)) Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
            }
            Leaders = new List<Leaders>();
        }
    }
}