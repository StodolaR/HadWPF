using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using static System.Formats.Asn1.AsnWriter;
using ShPath = System.Windows.Shapes.Path;

namespace HadWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int snakePartsDistance = 5;
        private const int snakePartRadius = 6;
        private const int snakePartStroke = 3;
        private const int snakeHeadRadius = 10;
        private const int foodRadius = 6;
        private const int stoneRadius = 10;
        private const int tailPartsCount = 4;
        private const int headPartsCount = 2;
        private const int minSpeedInterval = 40;
        private double coordX;
        private double coordY;
        private int angle;
        private int score;
        private int length;
        private int speedInterval;
        private EllipseGeometry elBodyPart;
        private Random random;
        private DispatcherTimer timer;
        private List<Highscore> highscores;
        public MainWindow()
        {
            InitializeComponent();
            elBodyPart = new EllipseGeometry(new Point(0, 0), snakePartRadius, snakePartRadius);
            highscores = new List<Highscore>();
            random = new Random();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            PlaceGrass();
            SetHighscores();
            icRanking.ItemsSource = highscores;
        }
        private void PlaceGrass()
        {
            Geometry liGrass = Geometry.Parse("M 0 0 L 5 20 V 0 V 20 L 10 0");
            for (int i = 0; i < 200; i++)
            {
                ShPath grass = new ShPath() { Data = liGrass, Stroke = Brushes.ForestGreen, StrokeThickness = 1 };
                PlacePathToCanvas(cnGrassBoard, grass, random.Next((int)cnGrassBoard.Width), random.Next((int)cnGrassBoard.Height));
            }
        }
        private void SetHighscores()
        {
            bool highscoresLoaded = false;
            if (File.Exists("highscores.xml"))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Highscore>));
                    using (FileStream fs = new FileStream("highscores.xml", FileMode.Open))
                    {
                        highscores = (List<Highscore>)serializer.Deserialize(fs);
                    }
                    highscoresLoaded = true;
                }
                catch
                {
                    highscoresLoaded = false;
                    MessageBox.Show("Nepodařilo se načíst nejlepší výsledky");
                }
            }
            if (highscoresLoaded == false) 
            {
                highscores = new List<Highscore>();
                for(int i = 0; i < 5; i++)
                {
                    highscores.Add(new Highscore { Name = "Nikdo", Score = 0 });
                }
            }
        }
        private void BeginGame(Border visibleBorder)
        {
            coordX = 250;
            coordY = 10;
            angle = 0;
            score = 0;
            window.Title = "Score: " + score;
            length = 30;
            speedInterval = 100;
            for (int i = 3; i < 6; i++)
            {
                PlacePathToCanvas(null, (ShPath)cnAppleBoard.Children[i], -20, 0);
            }
            cnAppleBoard.Children.RemoveRange(6, cnAppleBoard.Children.Count - 6);
            PlacePathToCanvas(null, pShadow, -random.Next(250), -random.Next(150));
            timer.Interval = TimeSpan.FromMilliseconds(speedInterval);
            visibleBorder.Visibility = Visibility.Collapsed;
            cnGrassBoard.Opacity = cnSnakeBoard.Opacity = cnAppleBoard.Opacity = 1;
            PlaceSneak();
            PlaceItem(new ShPath[] { pStalk, pAppleLeft, pAppleRight }, foodRadius);
            timer.Start();
        }
        private void PlacePathToCanvas(Canvas? canvas, ShPath path, double left, double top)
        {
            if (canvas != null)
            {
                canvas.Children.Add(path);
            }
            Canvas.SetLeft(path, left);
            Canvas.SetTop(path, top);
        }
        private void PlaceSneak()
        {
            cnSnakeBoard.Children.Clear();
            for (int radius = 4; radius < 8; radius++)
            {
                EllipseGeometry elTailPart = new EllipseGeometry(new Point(0, 0), radius, radius);
                ShPath tailPart = new ShPath() { Data = elTailPart, Fill = Brushes.Green };
                PlacePathToCanvas(cnSnakeBoard, tailPart, coordX, coordY);
                coordY += snakePartsDistance;
            }
            for (int i = 0; i < length - headPartsCount - tailPartsCount; i++)
            {
                ShPath bodyPart = new ShPath() { Data = elBodyPart, Fill = Brushes.Brown, Stroke = Brushes.Green, StrokeThickness = snakePartStroke };
                PlacePathToCanvas(cnSnakeBoard, bodyPart, coordX, coordY);
                coordY += snakePartsDistance;
            }
            cnSnakeBoard.Children.Remove(pHead);
            cnSnakeBoard.Children.Remove(pFace);
            PlacePathToCanvas(cnSnakeBoard, pHead, coordX, coordY);
            PlacePathToCanvas(cnSnakeBoard, pFace, coordX, coordY);
        }
        private void PlaceItem(ShPath[] itemParts, int itemRadius)
        {
            double distance = 0;
            double collisionDistance = snakeHeadRadius + itemRadius;
            while (distance < collisionDistance)
            {
                double itemCoordX = random.Next(10, (int)cnAppleBoard.Width - 10);
                double itemCoordY = random.Next(10, (int)cnAppleBoard.Height - 10);
                for (int i = 0; i < itemParts.Length; i++)
                {
                    PlacePathToCanvas(null, itemParts[i], itemCoordX, itemCoordY);
                }
                foreach(ShPath snakePart in cnSnakeBoard.Children)
                {
                    if ((distance = GetDistance(itemParts[0], snakePart)) < collisionDistance) break;
                }
            }
        }
        private double GetDistance(ShPath item1, ShPath Item2)
        {
            double distanceX = Canvas.GetLeft(item1) - Canvas.GetLeft(Item2);
            double distanceY = Canvas.GetTop(item1) - Canvas.GetTop(Item2);
            return Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }
        private void window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && spNewHighscore.Visibility == Visibility.Collapsed)
            {
                if (brdIntro.Visibility == Visibility.Visible)
                {
                    BeginGame(brdIntro);
                }
                else if (brdOutro.Visibility == Visibility.Visible)
                {
                    ShowHighscores();
                }
                else if (brdHighscores.Visibility == Visibility.Visible)
                {
                    BeginGame(brdHighscores);
                }
                else
                {
                    if(timer.IsEnabled)
                    {
                        timer.Stop(); 
                    }
                    else
                    {
                        timer.Start();
                    }
                }
            }
            else if (e.Key == Key.Escape && brdHighscores.Visibility == Visibility.Visible)
            {
                window.Close();
            }
        }
        private void ShowHighscores()
        {
            brdOutro.Visibility = Visibility.Collapsed;
            icRanking.Items.Refresh();
            brdHighscores.Visibility = Visibility.Visible;
        }
        private void SaveHighscores()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Highscore>));
                using (FileStream fs = new FileStream("highscores.xml", FileMode.Create))
                {
                    serializer.Serialize(fs, highscores);
                }
            }
            catch
            {
                MessageBox.Show("Výsledky se nepodařilo uložit");
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            SneakMove();
            RandomPlaceWrongFood(random.Next(1000));
            CheckColisions();
        }
        private void SneakMove()
        {
            coordX += (Math.Sin(angle * Math.PI / 180)) * snakePartsDistance;
            coordY += (Math.Cos(angle * Math.PI / 180)) * snakePartsDistance;
            ShPath movedPart;
            if (cnSnakeBoard.Children.Count >= length)
            {
                for (int i = 0; i < tailPartsCount; i++)
                {
                    Canvas.SetLeft(cnSnakeBoard.Children[i], Canvas.GetLeft(cnSnakeBoard.Children[i + 1]));
                    Canvas.SetTop(cnSnakeBoard.Children[i], Canvas.GetTop(cnSnakeBoard.Children[i + 1]));
                }
                movedPart = (ShPath)cnSnakeBoard.Children[tailPartsCount];
                cnSnakeBoard.Children.Remove(movedPart);
            }
            else
            {
                movedPart = new ShPath() { Data = elBodyPart, Fill = Brushes.Brown, Stroke = Brushes.Green, StrokeThickness = snakePartStroke };
            }
            cnSnakeBoard.Children.Insert(cnSnakeBoard.Children.Count - headPartsCount, movedPart);
            PlacePathToCanvas(null, movedPart, Canvas.GetLeft(pHead), Canvas.GetTop(pHead));
            PlacePathToCanvas(null, pHead, coordX, coordY);
            PlacePathToCanvas(null, pFace, coordX, coordY);
            if (Keyboard.IsKeyDown(Key.Left))
            {
                angle += 10;
            }
            if (Keyboard.IsKeyDown(Key.Right))
            {
                angle -= 10;
            }
            rtHead.Angle = rtFace.Angle = -angle;
        }
        private void RandomPlaceWrongFood(int wrongFoodSelector)
        {
            if (wrongFoodSelector < 3 && Canvas.GetLeft(pWrongAppleLeft) == -20)
            {
                PlaceItem(new ShPath[] { pWrongStalk, pWrongAppleLeft, pWrongAppleRight }, foodRadius);
            }
            else if (wrongFoodSelector > 3 && wrongFoodSelector < 6)
            {
                PlaceStone();
            }
        }
        private void PlaceStone()
        {
            EllipseGeometry elStone = new EllipseGeometry(new Point(0, 0), 11, 9);
            RotateTransform stoneRotate = new RotateTransform(random.Next(180));
            ShPath stone = new ShPath() { Data = elStone, Fill = Brushes.Gray, RenderTransform = stoneRotate };
            cnAppleBoard.Children.Add(stone);
            PlaceItem(new ShPath[] { stone }, stoneRadius);
        }
        private void CheckColisions()
        {
            CheckAppleCollisions();
            string checkResult = CheckEndCollisions();
            if(checkResult != string.Empty)
            {
                timer.Stop();
                tbEndReason.Text = checkResult;
                FinishGame();
            }
        }
        private void CheckAppleCollisions()
        {
            double distance;
            double collisionDistance = snakeHeadRadius + foodRadius;
            distance = GetDistance(pHead, pAppleLeft);
            if (distance < collisionDistance)
            {
                EatFood(1);
            }
            distance = GetDistance(pHead, pWrongAppleLeft);
            if (distance < collisionDistance)
            {
                EatFood(-5);
            }
        }
        private void EatFood(int point)
        {
            score += point;
            window.Title = "Score: " + score;
            if (speedInterval > minSpeedInterval)
            {
                speedInterval -= 2;
            }
            timer.Interval = TimeSpan.FromMilliseconds(speedInterval);
            length += 10;
            if (point > 0)
            {
                PlaceItem(new ShPath[] { pStalk, pAppleLeft, pAppleRight }, foodRadius);
            }
            else
            {
                PlacePathToCanvas(null, pWrongStalk, -20, 0);
                PlacePathToCanvas(null, pWrongAppleLeft, -20, 0);
                PlacePathToCanvas(null, pWrongAppleRight, -20, 0);
            }
        }
        private string CheckEndCollisions()
        {
            double distance;
            double collisionDistance = snakePartRadius + snakePartStroke + snakeHeadRadius - 2;
            string endString = "Auu... tak po tomhle už hada zcela přešla chuť na jablíčka a odplazil se raději do bezpečí.";
            for (int i = 0; i < cnSnakeBoard.Children.Count - 35; i++)
            {
                distance = GetDistance(pHead, (ShPath)cnSnakeBoard.Children[i]);
                if (distance < collisionDistance)
                {
                    return endString.Insert(20, " zakousnuti se do vlastního těla");
                }
            }
            if (coordX <= snakeHeadRadius || coordY <= snakeHeadRadius || coordX >= cnSnakeBoard.Width - snakeHeadRadius
                || coordY >= cnSnakeBoard.Height - snakeHeadRadius)
            {
                return endString.Insert(20, " nárazu hlavou do ohrady");
            }
            collisionDistance = snakeHeadRadius + stoneRadius;
            for (int stoneIndex = 6; stoneIndex < cnAppleBoard.Children.Count; stoneIndex++)
            {
                distance = GetDistance(pHead, (ShPath)cnAppleBoard.Children[stoneIndex]);
                if (distance < collisionDistance)
                {
                    return endString.Insert(20, " zakousnuti se do kamene");
                }
            }
            return string.Empty;
        }
        private void FinishGame()
        {
            tbScore.Text = score.ToString();
            cnGrassBoard.Opacity = cnSnakeBoard.Opacity = cnAppleBoard.Opacity = 0.3;
            brdOutro.Visibility = Visibility.Visible;
            if (highscores.Any(x => x.Score < score))
            {
                spWithoutHighscore.Visibility = Visibility.Collapsed;
                spNewHighscore.Visibility = Visibility.Visible;
                tbxWinnerName.Focus();
            }
        }
        private void btnSaveHighscore_Click(object sender, RoutedEventArgs e)
        {
            if (tbxWinnerName.Text == string.Empty) return;
            for (int i = 0; i < highscores.Count; i++)
            {
                if (score > highscores[i].Score)
                {
                    highscores.Insert(i, new Highscore { Name = tbxWinnerName.Text, Score = score });
                    highscores.RemoveAt(5);
                    SaveHighscores();
                    break;
                }
            }
            spWithoutHighscore.Visibility = Visibility.Visible;
            spNewHighscore.Visibility = Visibility.Collapsed;
            ShowHighscores();
        } 
    }
}