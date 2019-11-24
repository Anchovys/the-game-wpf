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

namespace the_game_wpf
{
    public class Item
    {
        public const int BlockSizeInPixelsX = 10; // размеры блока в пикселях по X
        public const int BlockSizeInPixelsY = 10; // размеры блока в пикселях по Y

        public Point GetAbsolutePositionByCoordinates(int x, int y)
        {
            return new Point(BlockSizeInPixelsX * x, BlockSizeInPixelsY * y);
        }
        public Point GetCoordinatesByAbsolutePosition(int x, int y)
        {
            return new Point(x / BlockSizeInPixelsX, y / BlockSizeInPixelsY);
        }
        public Rectangle MakeRectangle(Brush color)
        {
            Rectangle temporaryRectangle = new Rectangle();
            temporaryRectangle.Width = BlockSizeInPixelsX;
            temporaryRectangle.Height = BlockSizeInPixelsY;
            temporaryRectangle.Fill = color;

            return temporaryRectangle;
        }
    }
    public class GameObject : Item
    {
        public Point Position = new Point();
        public Rectangle Figure = new Rectangle();
    }
    public class WallObject : GameObject
    {
        public const char InitChar = '+';
        public WallObject(Point startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.Black);
        }
    }
    public class ExitObject : GameObject
    {
        public const char InitChar = '=';
        public ExitObject(Point startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.Green);
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        public void FillBuffer()
        {

        }
    }
}
