using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Common;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public int Stepcadr = 0;

        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            // Добавь для отладки
            if (MainWindow.mainWindow.ViewModelGames == null)
            {
                Debug.WriteLine("ViewModelGames = null");
                return;
            }

            if (MainWindow.mainWindow.ViewModelGames.SnakesPlayers == null)
            {
                Debug.WriteLine("SnakesPlayers = null");
                return;
            }

            Debug.WriteLine($"Отрисовка змеи: {MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count} точек");
            Dispatcher.Invoke(() =>
            {
                // Смена кадра
                if (Stepcadr == 0)
                    Stepcadr = 1;
                else
                    Stepcadr = 0;

                // Очищаем canvas
                canvas.Children.Clear();

                // Проверка на null
                if (MainWindow.mainWindow.ViewModelGames?.SnakesPlayers?.Points == null)
                    return;

                // Перебираем точки змеи
                for (int iPoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 1; iPoint >= 0; iPoint--)
                {
                    Snakes.Point SnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint];

                    // Анимация для не головы
                    if (iPoint != 0)
                    {
                        Snakes.Point NextSnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint - 1];

                        // Если точка находится по горизонтали
                        if (SnakePoint.X > NextSnakePoint.X || SnakePoint.X < NextSnakePoint.X)
                        {
                            if (iPoint % 2 == 0)
                            {
                                if (Stepcadr % 2 == 0)
                                    SnakePoint.Y -= 1;
                                else
                                    SnakePoint.Y += 1;
                            }
                            else
                            {
                                if (Stepcadr % 2 == 0)
                                    SnakePoint.Y += 1;
                                else
                                    SnakePoint.Y -= 1;
                            }
                        }
                        // Если точка находится по вертикали
                        else if (SnakePoint.Y > NextSnakePoint.Y || SnakePoint.Y < NextSnakePoint.Y)
                        {
                            if (iPoint % 2 == 0)
                            {
                                if (Stepcadr % 2 == 0)
                                    SnakePoint.X -= 1;
                                else
                                    SnakePoint.X += 1;
                            }
                            else
                            {
                                if (Stepcadr % 2 == 0)
                                    SnakePoint.X += 1;
                                else
                                    SnakePoint.X -= 1;
                            }
                        }
                    }

                    // Цвет для точки
                    Brush color;
                    if (iPoint == 0)
                        color = new SolidColorBrush(Color.FromArgb(255, 0, 127, 14)); // голова
                    else
                        color = new SolidColorBrush(Color.FromArgb(255, 0, 198, 19)); // тело

                    // Рисуем сегмент змеи
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                        Fill = color,
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(ellipse);
                }

                // Отрисовка яблока

                if (MainWindow.mainWindow.ViewModelGames?.Points != null)
                {
                    ImageBrush myBrush = new ImageBrush();
                    myBrush.ImageSource = new BitmapImage(new Uri("C:\\Users\\ЗС\\Source\\Repos\\Snake\\SnakeWPF\\Image\\apple.png"));

                    Ellipse points = new Ellipse()
                    {
                        Width = 40,
                        Height = 40,
                        Margin = new Thickness(
                            MainWindow.mainWindow.ViewModelGames.Points.X - 20,
                            MainWindow.mainWindow.ViewModelGames.Points.Y - 20, 0, 0),
                        Fill = myBrush
                    };
                    canvas.Children.Add(points);
                }
            });
        }
    }
}