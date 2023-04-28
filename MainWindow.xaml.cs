using Microsoft.VisualBasic;
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
    public partial class MainWindow : Window //Inheritance
    {
        //Dictionary that maps grid values to image sources
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            //If grid position is empty show empty image
            {GridValue.Empty,Images.Empty },
            //If grid position contains snake  show body image
            {GridValue.Snake,Images.Body },
            //If grid pos has food, show food image
            {GridValue.Food,Images.Food }

        };
        //Dictionary to contain snake eye rotion directions its moving in

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            {Direction.Up, 0},
            {Direction.Right,90 },
            {Direction.Down,180 },
            {Direction.Left,270 }

        };

        // Variables for Rows and Columns
        private readonly int rows = 20;
        private readonly int cols = 20;
        //Image table for image controls (Access image at given position in the grid)
        private readonly Image[,] gridImages;
        //GameState object
        private GameState gameState;
        private bool gameRunning;

        //Class Constructor Interaction logic for MainWindow.xaml
        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);  
        }
        //method  to run the game. 
        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            //Creating a new game state once game is over
            gameState = new GameState(rows,cols);
        }
        //Event handler to start the game when a key is pressed.
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }
            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }

        }
        //Event handler to handle keyboard keys.
        private void Window_KeyDown(object sender , KeyEventArgs e)
        {
            if (gameState.Gameover)
            {
                return; //Nothing happens when game over and we press key
            }
            // We will use arrow keyboard keys to move ou snake if the game is not over
            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;

            }

        }

        //Async loop method to move our snake at regular intervals
        //The loop will run until the game is over
        private async Task GameLoop()
        {
            while (!gameState.Gameover)
            {
                await Task.Delay(100); //Gamespeed
                gameState.Move();
                Draw();
            }
        }
        //Method to set up our Grid
        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            //Number of rows and cols on the game Grid
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            //To make sure the with ratio is good if we change our height or width
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);
            //Looping thru all our grid positions
            for(int i= 0; i < rows; i++)
            {
                for(int j= 0; j < cols; j++)
                {
                    //Giving grid positions empty images
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5,0.5)
                    };
                    //Storing our image in the our images table
                    images[i, j] = image;
                    GameGrid.Children.Add(image);//add it as a child
                }
            }
            return images;
        }


        //Methods to update our grid to reflect the current game state.
        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameState.Score}";// to update score
        }
        //Methods to update our grid Images to reflect current game state 
        private void DrawGrid()
        {
            for(int i=0; i < rows; i++)
            {
                for(int j=0; j < cols; j++)
                {
                    GridValue gridVal = gameState.Grid[i, j];
                    gridImages[i, j].Source = gridValToImage[gridVal]; //The dictionary is the source here
                    gridImages[i, j].RenderTransform = Transform.Identity;
                }
            }
        }
        //Method to draw snake head
        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;
            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }
        //Method to draw a dead snake
        private  async Task DrawDeadSnake()
        {
            //List with all snake positions
            List<Position> positions = new List<Position>(gameState.SnakePositions());
            for(int i=0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                //Getting images ofour dead snake
                ImageSource source =(i==0)?Images.DeadHead :Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }
        // Method to show count down before the game starts
        private async Task ShowCountDown()
        {
            for(int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }

        }

        //Method to show that the game is over
        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO START";
        }


       
    }
}
