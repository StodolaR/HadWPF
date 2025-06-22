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
        private double coordX = 250;
        private double coordY = 10;
        private int angle = 0;
        private int score = 0;
        private int length = 30;
        private int speedInterval = 100;
        private EllipseGeometry elBodyPart;
        private Random random = new Random();
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            PlaceSneak();
            PlaceFood(new Path[] {pStalk, pAppleLeft, pAppleRight});
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(speedInterval);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void PlaceSneak()
        {
            for (int radius = 4; radius < 8; radius++)
            {
                EllipseGeometry elTailPart = new EllipseGeometry(new Point(0, 0), radius, radius);
                Path tailPart = new Path() { Data = elTailPart, Fill = Brushes.Green };
                PlacePathToCanvas(cnSnakeBoard, tailPart, coordX, coordY);
                coordY += snakePartsDistance;
            }
            elBodyPart = new EllipseGeometry(new Point(0, 0), snakePartRadius, snakePartRadius);
            for (int i = 0; i < length - headPartsCount - tailPartsCount; i++)
            {
                Path bodyPart = new Path() { Data = elBodyPart, Fill = Brushes.Brown, Stroke = Brushes.Green, StrokeThickness = snakePartStroke };
                PlacePathToCanvas(cnSnakeBoard, bodyPart, coordX, coordY);
                coordY += snakePartsDistance;
            }
            cnSnakeBoard.Children.Remove(pHead);
            cnSnakeBoard.Children.Remove(pFace);
            PlacePathToCanvas(cnSnakeBoard, pHead, coordX, coordY);
            PlacePathToCanvas(cnSnakeBoard, pFace, coordX, coordY);
        }
        private void PlacePathToCanvas(Canvas? canvas, Path path, double left, double top)
        {
            if (canvas != null)
            {
                canvas.Children.Add(path);
            }
            Canvas.SetLeft(path, left);
            Canvas.SetTop(path, top);
        }
        private void PlaceFood(Path[] foodParts)
        {
            double distance = 0;
            double collisionDistance = snakeHeadRadius + foodRadius;
            while (distance < collisionDistance)
            {
                double foodCoordX = random.Next(10, (int)cnAppleBoard.Width - 10);
                double foodCoordY = random.Next(10, (int)cnAppleBoard.Height - 10);
                PlacePathToCanvas(null, foodParts[0], foodCoordX, foodCoordY);
                PlacePathToCanvas(null, foodParts[1], foodCoordX, foodCoordY);
                PlacePathToCanvas(null, foodParts[2], foodCoordX, foodCoordY);
                foreach(Path snakePart in cnSnakeBoard.Children)
                {
                    if ((distance = GetDistance(pAppleLeft, snakePart)) < collisionDistance) break;
                }
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
            Path movedPart;
            if (cnSnakeBoard.Children.Count >= length)
            {
                for (int i = 0; i < tailPartsCount; i++)
                {
                    Canvas.SetLeft(cnSnakeBoard.Children[i], Canvas.GetLeft(cnSnakeBoard.Children[i + 1]));
                    Canvas.SetTop(cnSnakeBoard.Children[i], Canvas.GetTop(cnSnakeBoard.Children[i + 1]));
                }
                movedPart = (Path)cnSnakeBoard.Children[tailPartsCount];
                cnSnakeBoard.Children.Remove(movedPart);
            }
            else
            {
                movedPart = new Path() { Data = elBodyPart, Fill = Brushes.Brown, Stroke = Brushes.Green, StrokeThickness = snakePartStroke };
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
                PlaceFood(new Path[] { pWrongStalk, pWrongAppleLeft, pWrongAppleRight });
            }
            else if (wrongFoodSelector > 3 && wrongFoodSelector < 5)
            {
                PlaceStone();
            }
        }
        private void PlaceStone()
        {
            EllipseGeometry elStone = new EllipseGeometry(new Point(0, 0), 11, 9);
            RotateTransform stoneRotate = new RotateTransform(random.Next(180));
            Path stone = new Path() { Data = elStone, Fill = Brushes.Gray, RenderTransform = stoneRotate };
            PlacePathToCanvas(cnAppleBoard, stone, random.Next((int)cnAppleBoard.Width), random.Next((int)cnAppleBoard.Height));
        }
        private void CheckColisions()
        {
            CheckEndCollisions();
            CheckAppleCollisions();
        }
        private void CheckEndCollisions()
        {
            double distance;
            double collisionDistance = snakePartRadius + snakePartStroke + snakeHeadRadius - 1;
            for (int i = 0; i < cnSnakeBoard.Children.Count - 35; i++)
            {
                distance = GetDistance(pHead, (Path)cnSnakeBoard.Children[i]);
                if (distance < collisionDistance)
                {
                    timer.Stop();
                    MessageBox.Show("Kusl ses do ocasa");
                    return;
                }
            }
            if (coordX <= snakeHeadRadius || coordY <= snakeHeadRadius || coordX >= cnSnakeBoard.Width - snakeHeadRadius
                || coordY >= cnSnakeBoard.Height - snakeHeadRadius)
            {
                timer.Stop();
                MessageBox.Show("Narvals do pangejtu");
                return;
            }
            collisionDistance = snakeHeadRadius + stoneRadius;
            for (int stoneIndex = 6; stoneIndex < cnAppleBoard.Children.Count; stoneIndex++)
            {
                distance = GetDistance(pHead, (Path)cnAppleBoard.Children[stoneIndex]);
                if (distance < collisionDistance)
                {
                    timer.Stop();
                    MessageBox.Show("Najebals šutr");
                }
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
                EatFood(-2);
            }
        }
        private void EatFood(int point)
        {
            score += point;
            speedInterval -= 2;
            timer.Interval = TimeSpan.FromMilliseconds(speedInterval);
            window.Title = "Score: " + score;
            length += 10;
            if (point > 0)
            {
                PlaceFood(new Path[] { pStalk, pAppleLeft, pAppleRight });
            }
            else
            {
                PlacePathToCanvas(null, pWrongStalk, -20, 0);
                PlacePathToCanvas(null, pWrongAppleLeft, -20, 0);
                PlacePathToCanvas(null, pWrongAppleRight, -20, 0);
            }
        }
        private double GetDistance(Path item1, Path Item2)
        {
            double distanceX = Canvas.GetLeft(item1) - Canvas.GetLeft(Item2);
            double distanceY = Canvas.GetTop(item1) - Canvas.GetTop(Item2);
            return Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }
    }
}