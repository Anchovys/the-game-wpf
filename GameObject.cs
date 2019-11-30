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
        public Ellipse MakeCurcle(Brush color)
        {
            Ellipse temporaryRectangle = new Ellipse();
            temporaryRectangle.Width = BlockSizeInPixelsX;
            temporaryRectangle.Height = BlockSizeInPixelsY;
            temporaryRectangle.Fill = color;
            return temporaryRectangle;
        }
    }
    public class GameObject : Item
    {
        public MyPoint Position = new MyPoint();
        public UIElement Figure = new UIElement();
        public Map MyMap;
        public GameController Controller;
        
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
            Figure = MakeCurcle(Brushes.Gold);
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
    public class EnemyObject : GameObject
    {
        public byte AttackZoneJumping = 2;  // область атаки прыжка
        public byte AttackZone = 1;         // область атаки ближний бой
        public byte ViewZone = 10;          // область видимости
        public GameObject Target;            // обьект, который выбран для преследования
        public const char InitChar = '?';
        public EnemyObject() { }
        public EnemyObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeRectangle(Brushes.Black);
        }
        /// <summary>
        /// Движение монстра к какому-либо обьекту
        /// </summary>
        /// <param name="targetPos">Позиция обьекта</param>
        public void MoveToTarget(MyPoint targetPos)
        {
            var newCoors = new MyPoint(Position);
            bool move = false;

            // Движения "по-тупому"
            {
                if (targetPos.X < Position.X)
                {
                    newCoors.X--;
                    move = true;
                }
                else if (targetPos.X > Position.X)
                {
                    newCoors.X++;
                    move = true;
                }

                if (!move)
                    if (targetPos.Y < Position.Y)
                        newCoors.Y--;
                    else if (targetPos.Y > Position.Y)
                        newCoors.Y++;
            }

            if (MyMap.GetByCoords(newCoors) is null)
                Move(newCoors);
        }

        /// <summary>
        /// Обработка коллизий
        /// </summary>
        public void CheckCollision()
        {
            foreach (var item in Position.GetNearPoints(AttackZoneJumping, true))
            {
                GameObject tObject = MyMap.GetByCoords(item);
                if (tObject is HeroObject)
                    Controller.ChangeState(false);
            }
            foreach (var item in Position.GetNearPoints(AttackZone, false))
            {
                GameObject tObject = MyMap.GetByCoords(item);
                if (tObject is EnemyObject && tObject != this)
                    Destroy();
            }
        }

        /// <summary>
        /// Хендлер перемещения врага к обьекту
        /// </summary>
        public void Move()
        {
            if (Target != null)
                MoveToTarget(Target.Position);

            // найти самый ближайший обьект игрок или монстр
            GameObject localObject = MyMap.FindNearlyObject(
                new GameObject[] { new EnemyObject(), new HeroObject() },
                Position, Position.GetNearPoints(ViewZone, false));

            if (localObject != this) // не должен быть текущим обьектом, в случае нахождения Enemy
                Target = localObject;
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

            GameObject inPathObject = MyMap.GetByCoords(newCoors);

            if (inPathObject is WallObject)
                return false;

            if (inPathObject is CoinObject)
                Controller.ChangeState(false);

            Move(newCoors);

            return true;
        }
    }
}