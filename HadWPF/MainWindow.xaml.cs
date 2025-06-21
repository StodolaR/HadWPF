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
        private const int startSpeedInterval = 200;
        private const int snakePartsDistance = 5;
        private const int snakePartRadius = 6;
        private const int tailPartsCount = 4;
        private const int headPartsCount = 2;
        private double coordX = 250;
        private double coordY = 10;
        private int angle = 0;
        private int score = 0;
        private int length = 30;
        private EllipseGeometry elBodyPart;
        private List<Path> apple = new List<Path>();
        private List<Path> wrongApple = new List<Path>();
        private Random random = new Random();
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            apple.Add(pStalk);
            apple.Add(pAppleLeft);
            apple.Add(pAppleRight);
            wrongApple.Add(pWrongStalk);
            wrongApple.Add(pWrongAppleLeft);
            wrongApple.Add(pWronfAppleRight);
            for (int radius = 4; radius < 8; radius++)
            {
                EllipseGeometry elTailPart = new EllipseGeometry(new Point (0,0), radius, radius);
                Path tailPart = new Path() { Data = elTailPart, Fill = Brushes.Green };
                cnSnakeBoard.Children.Add(tailPart);
                Canvas.SetLeft(tailPart, coordX);
                Canvas.SetTop(tailPart, coordY);
                coordY += snakePartsDistance;
            }
            elBodyPart = new EllipseGeometry(new Point(0, 0), snakePartRadius, snakePartRadius);
            for (int i = 0; i < length - headPartsCount - tailPartsCount; i++)
            {
                Path bodyPart = new Path() { Data = elBodyPart, Fill = Brushes.Brown, Stroke = Brushes.Green, StrokeThickness = 3 };;
                cnSnakeBoard.Children.Add(bodyPart);
                Canvas.SetLeft(bodyPart, coordX);
                Canvas.SetTop(bodyPart, coordY);
                coordY += snakePartsDistance;
            }
            cnSnakeBoard.Children.Remove(pHead);
            cnSnakeBoard.Children.Add(pHead);
            cnSnakeBoard.Children.Remove(pFace);
            cnSnakeBoard.Children.Add(pFace);
            Canvas.SetLeft(pHead, coordX);
            Canvas.SetTop(pHead, coordY);
            Canvas.SetLeft(pFace, coordX);
            Canvas.SetTop(pFace, coordY);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(startSpeedInterval);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void PlacePathToCanvas(Canvas canvas, Path path, double left, double top)
        {
            canvas.Children.Add(path);
            Canvas.SetLeft(path, left);
            Canvas.SetTop(path, top);
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            coordX += (Math.Sin(angle * Math.PI / 180)) * snakePartsDistance;
            coordY += (Math.Cos(angle * Math.PI / 180)) * snakePartsDistance;
            for(int i = 0;i < tailPartsCount ; i++)
            {
                Canvas.SetLeft(cnSnakeBoard.Children[i], Canvas.GetLeft(cnSnakeBoard.Children[i + 1]));
                Canvas.SetTop(cnSnakeBoard.Children[i], Canvas.GetTop(cnSnakeBoard.Children[i + 1]));
            }
            Path movedPart;
            if (cnSnakeBoard.Children.Count >= length)
            {
                movedPart = (Path)cnSnakeBoard.Children[tailPartsCount];
                cnSnakeBoard.Children.Remove(movedPart);  
            }
            else
            {
                movedPart = new Path() { Data = elBodyPart, Fill = Brushes.Brown, Stroke = Brushes.Green, StrokeThickness = 3 }; ;
            }
            cnSnakeBoard.Children.Insert(cnSnakeBoard.Children.Count - headPartsCount, movedPart);
            Canvas.SetLeft(movedPart, Canvas.GetLeft(pHead));
            Canvas.SetTop(movedPart, Canvas.GetTop(pHead));
            Canvas.SetLeft(pHead, coordX);
            Canvas.SetTop(pHead, coordY);
            Canvas.SetLeft(pFace, coordX);
            Canvas.SetTop(pFace, coordY);
        }
    }
}