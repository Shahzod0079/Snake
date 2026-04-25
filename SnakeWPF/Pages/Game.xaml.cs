using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Common;

namespace SnakeWPF.Pages
{
    public partial class Game : Page
    {
        public int Stepcadr = 0;

        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            Dispatcher.Invoke(() =>
            {
                Stepcadr = (Stepcadr == 0) ? 1 : 0;

                canvas.Children.Clear();

                var viewModel = MainWindow.mainWindow.ViewModelGames;
                if (viewModel == null) return;

                if (viewModel.SnakesPlayers?.Points != null)
                {
                    DrawSnake(viewModel.SnakesPlayers.Points, true);
                }

                if (viewModel.AllSnakes != null)
                {
                    var snakesCopy = viewModel.AllSnakes.ToList();
                    foreach (var otherSnake in snakesCopy)
                    {
                        if (otherSnake?.Points != null && otherSnake.Points.Count > 0)
                        {
                            DrawSnake(otherSnake.Points, false);
                        }
                    }
                }

                if (viewModel.Points != null)
                {
                    try
                    {
                        ImageBrush myBrush = new ImageBrush();
                        myBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/apple.png"));

                        Ellipse points = new Ellipse()
                        {
                            Width = 40,
                            Height = 40,
                            Margin = new Thickness(
                                viewModel.Points.X - 20,
                                viewModel.Points.Y - 20, 0, 0),
                            Fill = myBrush
                        };
                        canvas.Children.Add(points);
                    }
                    catch
                    {
                        Ellipse points = new Ellipse()
                        {
                            Width = 40,
                            Height = 40,
                            Margin = new Thickness(
                                viewModel.Points.X - 20,
                                viewModel.Points.Y - 20, 0, 0),
                            Fill = new SolidColorBrush(Colors.Red)
                        };
                        canvas.Children.Add(points);
                    }
                }
            });
        }

        private void DrawSnake(System.Collections.Generic.List<Snakes.Point> points, bool isMainPlayer)
        {
            var pointsCopy = points.ToList();

            for (int iPoint = pointsCopy.Count - 1; iPoint >= 0; iPoint--)
            {
                var SnakePoint = pointsCopy[iPoint];

                // анимация для не головы
                if (iPoint != 0 && pointsCopy.Count > iPoint)
                {
                    var NextSnakePoint = pointsCopy[iPoint - 1];

                    // горизонтальное движение
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
                    // вертикальное движение
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

                // цвет для точки
                Brush color;
                if (isMainPlayer)
                {
                    if (iPoint == 0)
                        color = new SolidColorBrush(Color.FromArgb(255, 0, 127, 14));
                    else
                        color = new SolidColorBrush(Color.FromArgb(255, 0, 198, 19));
                }
                else
                {
                    if (iPoint == 0)
                        color = new SolidColorBrush(Color.FromArgb(255, 14, 76, 127));
                    else
                        color = new SolidColorBrush(Color.FromArgb(255, 19, 98, 198));
                }

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
        }
    }
}