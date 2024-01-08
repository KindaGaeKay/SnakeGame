using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            {GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Bunny },
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };
        
    


        private readonly int rows = 15, cols = 15;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRuning;
        public int Players { get; set; } = 1;
        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
        }
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRuning)
            {
                if (e.Key == Key.D2)
                {
                    Players = 2;
                }
                else if (e.Key == Key.D1)
                {
                    Players = 1;
                }
                else
                {
                    return;
                }
                gameRuning = true;
                await RunGame();
                gameRuning = false;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            
            

            if (gameState.GameOver)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirectionSnake(Direction.Left); break;
                case Key.Right:
                    gameState.ChangeDirectionSnake(Direction.Right); break;
                case Key.Up:
                    gameState.ChangeDirectionSnake(Direction.Up); break;
                case Key.Down:
                    gameState.ChangeDirectionSnake(Direction.Down); break;
                case Key.E:
                    gameState.ChangeDirectionFood(Direction.None);  break;
                    

                    
            }
            if (Players == 2)
                    { switch (e.Key)
                {
                    case Key.A:
                        gameState.ChangeDirectionFood(Direction.Left); break;
                    case Key.D:
                        gameState.ChangeDirectionFood(Direction.Right); break;
                    case Key.W:
                        gameState.ChangeDirectionFood(Direction.Up); break;
                    case Key.S:
                        gameState.ChangeDirectionFood(Direction.Down); break;
                }
            }
            else
            {       
                switch (e.Key)
                {
                    case Key.A:
                        gameState.ChangeDirectionSnake(Direction.Left); break;
                    case Key.D:
                        gameState.ChangeDirectionSnake(Direction.Right); break;
                    case Key.W:
                        gameState.ChangeDirectionSnake(Direction.Up); break;
                    case Key.S:
                        gameState.ChangeDirectionSnake(Direction.Down); break;
                }
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                int spood = 0;
                int speedy =  100 - gameState.Score;
                if (speedy >= 1)
                {
                    spood = speedy;
                }
                await Task.Delay(spood);
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r,c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameState.Score}";
        }

        

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols ; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> position = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < position.Count; i++)
            {
                Position pos = position[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }
        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--) 
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "Press 1 for single player. Press 2 for 2 player.";
        }
    }
}
