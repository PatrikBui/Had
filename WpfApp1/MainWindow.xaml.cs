using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace WpfSnakeGame
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int VelikostPole = 20;
        private const int VelikostPolicka = 20;

        private const int MaxRychlost = 10;

        private List<Policko> _had;
        private List<Policko> _jidla;
        private Direction _smer;
        private bool _hraBezi;
        private DispatcherTimer _gameTimer;
        private int _rychlost;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Skore { get; private set; }

        public int Rychlost
        {
            get { return _rychlost; }
            set
            {
                _rychlost = Math.Min(value, MaxRychlost);
                if (_gameTimer != null)
                    _gameTimer.Interval = TimeSpan.FromMilliseconds(1000 / _rychlost);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeGame();
            StartHry();
        }

        private void InitializeGame()
        {
            HraciPole.Children.Clear();
            _had = new List<Policko>();
            _jidla = new List<Policko>();
            _smer = Direction.Right;
            _hraBezi = false;
            Skore = 0;
            Rychlost = 5;
            GenerovaniHada();
            GenerovaniJidla();
        }
       
        private void GenerovaniHada()
        {
            _had.Clear();
            _had.Add(new Policko(10, 10));

            foreach (var policko in _had)
            {
                AddPolickoToCanvas(policko, Brushes.Black);
            }
        }

        private void GenerovaniJidla()
        {
            Random random = new Random();
            int x, y;

            do
            {
                x = random.Next(0, VelikostPole);
                y = random.Next(0, VelikostPole);
            }
            while (_had.Any(policko => policko.X == x && policko.Y == y) || _jidla.Any(policko => policko.X == x && policko.Y == y));

            _jidla.Add(new Policko(x, y));
            foreach (var policko in _jidla)
            {
                AddPolickoToCanvas(policko, Brushes.Red);
            }
        }

        private void AddPolickoToCanvas(Policko policko, Brush barva = null)
        {
            Rectangle rect = new Rectangle
            {
                Width = VelikostPolicka,
                Height = VelikostPolicka,
                Fill = barva ?? Brushes.White
            };

            Canvas.SetLeft(rect, policko.X * VelikostPolicka);
            Canvas.SetTop(rect, policko.Y * VelikostPolicka);

            HraciPole.Children.Add(rect);
        }

        private void Pohyb()
        {
            if (!_hraBezi)
                return;

            Policko hlava = _had.First();
            int dx = 0;
            int dy = 0;

            switch (_smer)
            {
                case Direction.Left:
                    dx = -1;
                    break;
                case Direction.Right:
                    dx = 1;
                    break;
                case Direction.Up:
                    dy = -1;
                    break;
                case Direction.Down:
                    dy = 1;
                    break;
            }

            int newHeadX = (hlava.X + dx + VelikostPole) % VelikostPole;
            int newHeadY = (hlava.Y + dy + VelikostPole) % VelikostPole;

            Policko newHead = new Policko(newHeadX, newHeadY);

            if (_had.Any(policko => policko.X == newHead.X && policko.Y == newHead.Y))
            {
                Konec();
                return;
            }

            _had.Insert(0, newHead);
            if (_jidla.Any(policko => policko.X == newHead.X && policko.Y == newHead.Y))
            {
                Skore++;
                _jidla.RemoveAll(policko => policko.X == newHead.X && policko.Y == newHead.Y);
                GenerovaniJidla();
            }
            else
            {
                _had.RemoveAt(_had.Count - 1);
            }

            aktualizace();
        }

        private void aktualizace()
        {
            HraciPole.Children.Clear();

            for (int i = 0; i < VelikostPole; i++)
            {
                for (int j = 0; j < VelikostPole; j++)
                {
                    Brush barva = (i + j) % 2 == 0 ? Brushes.Green : Brushes.DarkGreen;
                    Rectangle rect = new Rectangle
                    {
                        Width = VelikostPolicka,
                        Height = VelikostPolicka,
                        Fill = barva
                    };

                    Canvas.SetLeft(rect, i * VelikostPolicka);
                    Canvas.SetTop(rect, j * VelikostPolicka);

                    HraciPole.Children.Add(rect);
                }
            }

            foreach (var policko in _had)
            {
                AddPolickoToCanvas(policko, Brushes.Black);
            }

            foreach (var policko in _jidla)
            {
                AddPolickoToCanvas(policko, Brushes.Red);
            }
        }

        private void Konec()
        {
            _hraBezi = false;
            MessageBox.Show($"Konec hry! Dosáhli jste skóre {Skore}.");
            InitializeGame();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_hraBezi)
                _hraBezi = true;

            switch (e.Key)
            {
                case Key.Left:
                    if (_smer != Direction.Right)
                        _smer = Direction.Left;
                    break;
                case Key.Right:
                    if (_smer != Direction.Left)
                        _smer = Direction.Right;
                    break;
                case Key.Up:
                    if (_smer != Direction.Down)
                        _smer = Direction.Up;
                    break;
                case Key.Down:
                    if (_smer != Direction.Up)
                        _smer = Direction.Down;
                    break;
            }
        }

        private void StartHry()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Tick += GameLoop;
            _gameTimer.Interval = TimeSpan.FromMilliseconds(1000 / Rychlost);
            _gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            Pohyb();
        }
    }

    public class Policko
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Policko(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
   
    
}
