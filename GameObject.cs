using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace the_game_wpf
{
    public class Item
    {
        public const int BlockSizeInPixelsX = 10; // размеры блока в пикселях по X
        public const int BlockSizeInPixelsY = 10; // размеры блока в пикселях по Y

        public MyPoint GetAbsolutePositionByCoordinates(MyPoint point)
        {
            return new MyPoint(BlockSizeInPixelsX * point.X, BlockSizeInPixelsY * point.Y);
        }
        public MyPoint GetCoordinatesByAbsolutePosition(int x, int y)
        {
            return new MyPoint(x / BlockSizeInPixelsX, y / BlockSizeInPixelsY);
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
        public MyPoint Position = new MyPoint();
        public Rectangle Figure = new Rectangle();
        public Map MyMap;

        public void Destroy()
        {
            MyMap.PlaceObject(Position, null, true);
        }

        public void Move(MyPoint newPosition) 
        {
            Destroy(); // уничтожим старый обьект
            Position = newPosition;  // поменяем позицию текущего
            MyMap.PlaceObject(newPosition, this, true); // запишем в новую
        }
    }
    public class WallObject : GameObject
    {
        public const char InitChar = '#';
        public WallObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.DarkBlue);
        }
    }
    public class CoinObject : GameObject
    {
        public const char InitChar = '0';
        public CoinObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.Gold);
        }
    }
    public class ExitObject : GameObject
    {
        public const char InitChar = '=';
        public ExitObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.LightGray);
        }
    }
    public class HeroObject : GameObject
    {
        public const char InitChar = '+';
        public HeroObject() { }
        public HeroObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.PaleGreen);
        }
        public bool Move(Key key)
        {
            var newCoors = new MyPoint(Position);

            switch (key)
            {
                case Key.W:
                case Key.Up:
                    newCoors.Y--;
                    break;
                case Key.S:
                case Key.Down:
                    newCoors.Y++;
                    break;
                case Key.A:
                case Key.Left:
                    newCoors.X--;
                    break;
                case Key.D:
                case Key.Right:
                    newCoors.X++;
                    break;
                default:
                    return false;
            }

            foreach (var item in MyMap.GameObjects)
            {
                if (item.Key.X == 1)
                {
                    if (item.Value != null)
                        Console.WriteLine("ТУТ: {0} ---> {1}", item.Key.String(), item.Value.GetType().ToString());
                    else Console.WriteLine("ТУТ: {0} ---> {1}", item.Key.String(), "ПУСТО");
                }
            }


            GameObject inPathObject = MyMap.GetByCoords(newCoors);

            Console.WriteLine("WANT GO ->" + newCoors.String());
            Console.ReadKey();


            if(MyMap.GameObjects.ContainsKey(newCoors))
                Console.WriteLine("HERE " + MyMap.GameObjects[newCoors].GetType().Name);
            else
                Console.WriteLine("HERE NULL");

            if (inPathObject is WallObject)
                return false;

            Move(newCoors);

            return true;
        }
    }
}