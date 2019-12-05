using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace the_game_wpf
{
    public class Item
    {
        // TODO: Идея - размеры блока в зависимости от размера карты
        public const int BlockSizeInPixelsX = 16; // размеры блока в пикселях по X
        public const int BlockSizeInPixelsY = 16; // размеры блока в пикселях по Y

        public MyPoint GetAbsolutePositionByCoordinates(MyPoint point)
        {
            return new MyPoint() { X = BlockSizeInPixelsX * point.X, Y = BlockSizeInPixelsY * point.Y };
        }
        public MyPoint GetCoordinatesByAbsolutePosition(int x, int y)
        {
            return new MyPoint() { X = x / BlockSizeInPixelsX, Y = y / BlockSizeInPixelsY };
        }
        public Rectangle MakeRectangle(Brush color)
        {
            Rectangle temporaryRectangle = new Rectangle
            {
                Width = BlockSizeInPixelsX,
                Height = BlockSizeInPixelsY,
                Fill = color
            };
            return temporaryRectangle;
        }
        public Ellipse MakeCurcle(Brush color)
        {
            Ellipse temporaryRectangle = new Ellipse
            {
                Width = BlockSizeInPixelsX,
                Height = BlockSizeInPixelsY,
                Fill = color
            };
            return temporaryRectangle;
        }

        public Rectangle MakeImage(string path)
        {
            string fullpath = System.IO.Path.Combine("Assets", path) + ".png";

            if (!System.IO.File.Exists(fullpath))
                throw new Exception("ERROR :: NOT FOUND IMAGE ON PATH: " + fullpath);

            Rectangle image = new Rectangle
            {
                Width = BlockSizeInPixelsX,
                Height = BlockSizeInPixelsY,
                Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(fullpath, UriKind.Relative)) },
                Stretch = Stretch.Fill
            };

            return image;
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
            Console.WriteLine("--> Obj {0} has breen removed from map ({1})", GetType().Name, Position.String());
            MyMap.PlaceObject(Position, null, true);
        }

        public void Move(MyPoint newPosition) 
        {
            Console.WriteLine("--> Obj {0} moved to ({1} --> {2})", GetType().Name, Position.String(), newPosition.String());
            Destroy(); // уничтожим старый обьект
            Position = newPosition;  // поменяем позицию текущего
            MyMap.PlaceObject(newPosition, this, true); // запишем в новую
        }
    }
    public class WallObject : GameObject
    {
        public const char InitChar = '#';   // символ, которым изображен этот обьект на карте
        public WallObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
    }
    public class CoinObject : GameObject
    {
        public const char InitChar = '0';   // символ, которым изображен этот обьект на карте
        public CoinObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
    }
    public class ExitObject : GameObject
    {
        public const char InitChar = '=';   // символ, которым изображен этот обьект на карте
        public ExitObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
    }
    public class EnemyObject : GameObject
    {
        public byte AttackZoneJumping = 2;  // область атаки прыжка
        public byte AttackZone = 1;         // область атаки ближний бой
        public byte ViewZone = 10;          // область видимости
        public GameObject Target;           // обьект, который выбран для преследования
        public const char InitChar = '?';   // символ, которым изображен этот обьект на карте
        public EnemyObject() { }
        public EnemyObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
        /// <summary>
        /// Движение монстра к какому-либо обьекту
        /// </summary>
        /// <param name="targetPos">Позиция обьекта</param>
        public void MoveToTarget(MyPoint targetPos)
        {
            var newCoors = new MyPoint() { X = Position.X, Y = Position.Y };
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
                {
                    Controller.ShowBox("Вас сожрали монстры — с кем не бывает?");
                    Controller.Stop();
                }
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

            // найти самый ближайший обьект (игрока или монстра)
            GameObject localObject = MyMap.FindNearlyObject(
                new GameObject[] { new EnemyObject(), new HeroObject() },
                Position, Position.GetNearPoints(ViewZone, false));

            if (localObject != this) // не должен быть текущим обьектом, в случае нахождения Enemy
                Target = localObject;
        }

    }
    public class HeroObject : GameObject
    {
        public const char InitChar = '+';   // символ, которым изображен этот обьект на карте
        public HeroObject() { }
        public HeroObject(MyPoint startPosition)
        {
            Position = startPosition;
            //Figure = MakeRectangle(Brushes.PaleGreen);
            Figure = MakeImage(GetType().Name);
        }
        public bool Move(Key key)
        {
            var newCoors = new MyPoint() { X = Position.X, Y = Position.Y };

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
            switch (inPathObject)
            {
                case WallObject _:
                    return false;
                case CoinObject _:
                    inPathObject.Move(new MyPoint() { X = -100 });
                    break;
                case ExitObject _:
                    Controller.ShowBox("Вы выиграли!");
                    Controller.Stop();
                    break;
            }
            Move(newCoors);
            return true;
        }
    }
}