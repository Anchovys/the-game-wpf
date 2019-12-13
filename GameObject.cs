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
        public GameController Controller;

        public int BlockSizeInPixelsX = 16; // размеры блока в пикселях по X
        public int BlockSizeInPixelsY = 16; // размеры блока в пикселях по Y

        /// <summary>
        /// Возвращает позицию обьекта в пикселях (верхнюю левую точку)
        /// </summary>
        /// <param name="point">Координаты обьекта</param>
        /// <returns>Позиция обьекта</returns>
        public MyPoint GetAbsolutePositionByCoordinates(MyPoint point)
        {
            return new MyPoint() { X = BlockSizeInPixelsX * point.X, Y = BlockSizeInPixelsY * point.Y };
        }

        /// <summary>
        /// Пытается найти координаты обьекта по переданым пикселям НАЧАЛА ОБЬЕКТА (верхняя левая точка).
        /// </summary>
        /// <param name="x">Позиция по X(в пикселях)</param>
        /// <param name="y">Позиция по Y(в пикселях)</param>
        /// <returns>Координаты обьекта</returns>
        public MyPoint GetCoordinatesByAbsolutePosition(int x, int y)
        {
            return new MyPoint() { X = x / BlockSizeInPixelsX, Y = y / BlockSizeInPixelsY };
        }

        public Rectangle MakeImage(string path, MyPoint changeSizes = null)
        {
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
            // полный путь. папка со спрайтами указывается через настройки
            string fullpath = string.Format("{0}{1}", RunningPath,
                System.IO.Path.Combine(System.IO.Path.Combine(Controller.Options.SpritesPath, "items"), path + ".png"));

            // нужно проверить, есть ли файл по пути
            if (!System.IO.File.Exists(fullpath))
                throw new Exception("ERROR :: NOT FOUND IMAGE ON PATH: " + fullpath);

            // заданы кастомные размеры
            if (changeSizes != null)
            {
                BlockSizeInPixelsX = (int)changeSizes.X;
                BlockSizeInPixelsY = (int)changeSizes.Y;
            }

            // создаем прямоугольник
            return new Rectangle
            {
                Width = BlockSizeInPixelsX,
                Height = BlockSizeInPixelsY,
                Fill = new ImageBrush  //вся "магия" - здесь. подставим картинку
                {
                    ImageSource = new BitmapImage(new Uri(fullpath, UriKind.Relative))
                },
                Stretch = Stretch.Fill
            };

        }
    }
    public class GameObject : Item
    {
        public MyPoint Position = new MyPoint();
        public UIElement Figure = new UIElement();

        // временный обьект, сохраняемый при перемещении обьекта
        private GameObject TempMoveObject = null;

        /// <summary>
        /// Убрать обьект с карты
        /// </summary>
        public void Destroy()
        {
            Console.WriteLine("--> Obj {0} removed [F] coords({1})", GetType().Name, Position.ToString());
            Controller.MainMap.PlaceObject(Position, null, true);
        }

        /// <summary>
        /// Переместить обьект куда-то, на другие координаты
        /// </summary>
        /// <param name="newPosition">Куда перемещать</param>
        public MyPoint Move(MyPoint newPosition, bool notForced = false)
        {
            if (Equals(newPosition, Position))
                return Position;

            if (!notForced)
            {
                Console.WriteLine("--> Obj {0} moved to (old : '{1}' --> new : '{2}') [F]", GetType().Name, Position.ToString(), newPosition.ToString());
                Destroy(); // уничтожим старый обьект
                Position = newPosition;  // поменяем позицию текущего
                Controller.MainMap.PlaceObject(newPosition, this, true); // запишем в новую
                return newPosition;
            }
            else
            {
                Destroy(); // уничтожим старый обьект
                Console.WriteLine("--> Obj {0} moved [N F] to (old : '{1}' --> new : '{2}')", GetType().Name, Position.ToString(), newPosition.ToString());
                if (TempMoveObject != null)
                {
                    TempMoveObject.Position = Position;
                    Controller.MainMap.PlaceObject(Position, TempMoveObject, true);
                    Console.WriteLine("==> N F FEATURE :: Placed Object '{0}'", TempMoveObject.GetType().FullName);
                    TempMoveObject = null;
                }
                
                // посмотрим что идет дальше
                GameObject next = Controller.MainMap.GetByCoords(newPosition);

                // не пустота и не игрок, запишем в буфер (сохраним)
                if (next != null && next.GetType() != typeof(HeroObject))
                {
                    Console.WriteLine("==> N F FEATURE :: Saved Object '{0}'", next.GetType().FullName);
                    TempMoveObject = next;
                }

                Controller.MainMap.PlaceObject(newPosition, this, true); // запишем в новую

                Position = newPosition;  // поменяем позицию текущего
                return newPosition;
            }
        }

        /// <summary>
        /// Размещает теущий экземпляр класса на экране
        /// Размешать через Dispatcher
        /// </summary>
        public void Place(bool replace = false)
        {
            Controller.Window.Dispatcher.Invoke(() =>
            {
                Controller.MainMap.PlaceObject(Position, this, replace);
            });
        }
    }
    public class OpenedDoorObject : GameObject
    {
        public const char InitChar = 'd';   // символ, которым изображен этот обьект на карте
        public OpenedDoorObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public OpenedDoorObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class ClosedDoorObject : GameObject
    {
        public const char InitChar = '~';   // символ, которым изображен этот обьект на карте
        public ClosedDoorObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public ClosedDoorObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class WallObject : GameObject
    {
        public const char InitChar = '#';   // символ, которым изображен этот обьект на карте
        public WallObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public WallObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class DestroyedWallObject : GameObject
    {
        public const char InitChar = 'w';   // символ, которым изображен этот обьект на карте
        public DestroyedWallObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public DestroyedWallObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition; 
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class KeyObject : GameObject
    {
        public const char InitChar = '/';   // символ, которым изображен этот обьект на карте
        public KeyObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public KeyObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class AmmoObject : GameObject
    {
        public const char InitChar = '8';   // символ, которым изображен этот обьект на карте
        public AmmoObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public AmmoObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class CannonObject : GameObject
    {
        public const char InitChar = 'C';   // символ, которым изображен этот обьект на карте
        public CannonObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public CannonObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class BulletObject : GameObject
    {
        public const char InitChar = '.';   // символ, которым изображен этот обьект на карте
        public bool LeftDirection = true;
        public byte BulletHealth = 3;

        public bool Freeze = false;
        public bool Clean = false;

        public BulletObject() { }
        public BulletObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public BulletObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }

        public void Move()
        {
            var newCoors = new MyPoint() { X = Position.X, Y = Position.Y };
            if (Freeze)
            {
                if (Clean) Destroy();
                return;
            }

            if (BulletHealth == 0)
            {
                Freeze = true;
                Clean = true;
                return;
            }

            if (LeftDirection)
                newCoors.X--;
            else newCoors.X++;

            if (!Controller.MainMap.CheckLimits(newCoors))
            {
                Freeze = true;
                Clean = true;
                return;
            }

            GameObject nextObject = Controller.MainMap.GetByCoords(newCoors);

            if (nextObject != null)
                switch (nextObject)
                {
                    case WallObject _:
                        {
                            BulletHealth--;
                            nextObject.Destroy();

                            Controller.Window.Dispatcher.Invoke(() =>
                            {
                                var destroyedWall = new DestroyedWallObject(newCoors, Controller);

                                destroyedWall.Place();
                            });

                            return;
                        }

                    case DemonObject _:
                        {
                            nextObject.Destroy();
                            DemonObject demonObject = nextObject as DemonObject;
                            demonObject.Kill();
                            return;
                        }

                    case TuxObject _:
                        {
                            nextObject.Destroy();
                            TuxObject tuxObject = nextObject as TuxObject;
                            tuxObject.Kill();
                            return;
                        }

                    case HeroObject _:
                        {
                            Destroy();
                            HeroObject heroObject = nextObject as HeroObject;
                            heroObject.Kill("Вас раздавило пушечным ядром.");
                            return;
                        }

                    case BulletObject _:
                        nextObject.Destroy();
                        Destroy();
                        return;

                    case CoinObject _:
                    case ClosedDoorObject _:
                    case KeyObject _:
                        // меняем направление пули
                        LeftDirection = !LeftDirection;
                        BulletHealth--;
                        return;
                }

            Move(newCoors, true);
        }
    }
    public class CoinObject : GameObject
    {
        public const char InitChar = '0';   // символ, которым изображен этот обьект на карте
        public CoinObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public CoinObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class ExitDoorObject : GameObject
    {
        public const char InitChar = '=';   // символ, которым изображен этот обьект на карте
        public ExitDoorObject() { }
        public ExitDoorObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public ExitDoorObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class DemonObject : GameObject
    {
        public byte AttackZoneJumping = 2;  // область атаки прыжка
        public byte AttackZone = 1;         // область атаки ближний бой
        public byte ViewZone = 10;          // область видимости
        public GameObject Target;           // обьект, который выбран для преследования
        public const char InitChar = '?';   // символ, которым изображен этот обьект на карте
        public DemonObject() { }
        public DemonObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public DemonObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }

        public void Kill() 
        {
            Controller.Window.Dispatcher.Invoke(() =>
            {
                var blood = new BloodObject(Position, Controller);
                blood.Place();
            });
        }

        private bool CheckMove(MyPoint сoors) 
        {
            GameObject obj = Controller.MainMap.GetByCoords(сoors);


            if (obj is WallObject || obj is ClosedDoorObject || obj is CoinObject || obj is BulletObject)
                return false;
            
            return true;
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

            if(CheckMove(newCoors))
                Move(newCoors, true);
        }

        /// <summary>
        /// Обработка коллизий для монстра
        /// </summary>
        public void CheckCollision()
        {
            foreach (var item in Position.GetNearPoints(AttackZoneJumping, true))
            {
                GameObject tObject = Controller.MainMap.GetByCoords(item);

                if(!Controller.MainMap.WallCheck(Position, item))
                {
                    if (tObject is HeroObject)
                    {
                        HeroObject heroObject = tObject as HeroObject;
                        heroObject.Kill("Вы были погрызаны демонами.");
                        return;
                    }
                    else if (tObject is TuxObject)
                    {
                        TuxObject tuxObject = tObject as TuxObject;
                        tuxObject.Kill();
                        return;
                    }
                }
            }
            foreach (var item in Position.GetNearPoints(AttackZone, false))
            {
                GameObject tObject = Controller.MainMap.GetByCoords(item);

                if (tObject is DemonObject && tObject != this)
                    Destroy();
            }
        }

        /// <summary>
        /// Хендлер перемещения врага к обьекту
        /// </summary>
        public void Move()
        {
            // обьект уже найден
            if (Target != null)
                MoveToTarget(Target.Position);

            // найти самый ближайший обьект (игрока или монстра)
            // TODO: Игрок в приоритете
            GameObject localObject = Controller.MainMap.FindNearlyObject(
                new GameObject[] { new TuxObject(), new HeroObject() },
                Position, Position.GetNearPoints(ViewZone, false));

            // не должен быть текущим обьектом, в случае нахождения Enemy
            if (localObject != this)
                Target = localObject;
        }

    }
    public class TuxObject : GameObject
    {
        public byte ExitZone = 2;           // область выхода
        public byte StopZone = 2;           // область атаки прыжка
        public byte ViewZone = 10;          // область поиска игрока
        public byte ViewZoneDoor = 7;       // область поиска двери
        public GameObject Target;           // обьект, который выбран для преследования
        public const char InitChar = 'T';   // символ, которым изображен этот обьект на карте
        public TuxObject() { }
        public TuxObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public TuxObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }

        public void Kill()
        {
            Destroy();
            Controller.Window.Dispatcher.Invoke(() =>
            {
                var blood = new BloodObject(Position, Controller);

                blood.Place();
            });

            Controller.ShowBox("Вы проиграли!\nОдин из пингвинов погиб.");
            Controller.Stop();
        }

        private bool CheckMove(MyPoint сoors)
        {
            GameObject obj = Controller.MainMap.GetByCoords(сoors);

            if (obj is WallObject || obj is ClosedDoorObject || obj is CoinObject ||
                obj is BulletObject || obj is TuxObject || obj is DemonObject)
                return false;

            return true;
        }

        /// <summary>
        /// Движение монстра к какому-либо обьекту
        /// </summary>
        /// <param name="targetPos">Позиция обьекта</param>
        public MyPoint MoveToTarget(MyPoint targetPos)
        {
            var newCoors = new MyPoint() { X = Position.X, Y = Position.Y };
            bool move = false;

            foreach (var item in Position.GetNearPoints(StopZone, true))
                if (Controller.MainMap.GetByCoords(item) is HeroObject)
                    return Position;

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

            if (CheckMove(newCoors))
            {
                return newCoors;
            }
            else
            {
                return Position;
            }
        }

        /// <summary>
        /// Хендлер перемещения пингвина к обьекту
        /// </summary>
        public void Move()
        {
            foreach (var item in Position.GetNearPoints(ExitZone, true))
                if (Controller.MainMap.GetByCoords(item) is ExitDoorObject)
                {
                    Destroy();
                    return;
                }

            if (Target is HeroObject)
            {
                GameObject obj = Controller.MainMap.FindNearlyObject(
                new GameObject[] { new ExitDoorObject() },
                Position, Position.GetNearPoints(ViewZoneDoor, false));
                
                if(obj != null)
                    Target = obj;
            }

            if (Target == null || Equals(MoveToTarget(Target.Position), Position))
            {
                // найти самый ближайший обьект (игрока или монстра)
                // TODO: Игрок в приоритете
                GameObject localObject = Controller.MainMap.FindNearlyObject(
                    new GameObject[] { new HeroObject() },
                    Position, Position.GetNearPoints(ViewZone, false));

                if (localObject != null)
                    Target = localObject;
            }

            if (Target != null)
                Move(MoveToTarget(Target.Position), true);
                
        }

    }
    public class BloodObject : GameObject
    {
        public const char InitChar = 'B';   // символ, которым изображен этот обьект на карте
        public BloodObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public BloodObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
    }
    public class HeroObject : GameObject
    {
        public const char InitChar = '+';   // символ, которым изображен этот обьект на карте
        public bool LeftDirection = false;
        public bool HasKey = false;
        public bool HasGun = false;
        public int Ammo = 0;

        public HeroObject() { }
        public HeroObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);
        }
        public HeroObject(MyPoint startPosition, GameController controller)
        {
            Position = startPosition;
            Controller = controller;
            Figure = MakeImage(GetType().Name, Controller.MainMap.BlockSizes);   
        }

        /// <summary>
        /// Убивает игрока
        /// </summary>
        public void Kill(string deathReason = "Вы умерли") 
        {
            Controller.Window.Dispatcher.Invoke(() =>
            {
                var blood = new BloodObject(Position, Controller);

                blood.Place(true);
            });

            Controller.Stop();
            Controller.ShowBox(deathReason);
        }

        /// <summary>
        /// Управляем игроком в зависимости от зажатой кнопки
        /// </summary>
        /// <param name="key">Зажатая кнопка для обработки</param>
        /// <returns>Успешно ли переместился игрок</returns>
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
                    LeftDirection = true;
                    break;
                case Key.D:
                case Key.Right:
                    newCoors.X++;
                    LeftDirection = false;
                    break;
                case Key.F:

                    if (HasGun && Ammo != 0)
                    {
                        Ammo--;
                        if (LeftDirection) newCoors.X--;
                        else newCoors.X++;

                        if (Controller.MainMap.GetByCoords(newCoors) != null)
                            return false;

                        Controller.Window.Dispatcher.Invoke(() =>
                        {
                            BulletObject bullet = new BulletObject(newCoors, Controller)
                            {
                                LeftDirection = LeftDirection,
                                Position = newCoors,
                            };
                            bullet.Place();
                        });
                    }
                    

                    return true;
                default:
                    return false;
            }

            GameObject inPathObject = Controller.MainMap.GetByCoords(newCoors);
            switch (inPathObject)
            {
                case ClosedDoorObject _:
                    // ключ есть
                    if (HasKey)
                    {
                        // испольюзуем ключ
                        HasKey = false;

                        inPathObject.Destroy();
                        Controller.Window.Dispatcher.Invoke(() =>
                        {
                            Controller.MainMap.PlaceObject(newCoors, new OpenedDoorObject(newCoors, Controller));
                        });
                        return false;
                    }
                    return false;

                case WallObject _:
                    return false;

                case BulletObject _:
                    return false;

                case KeyObject _:
                    if(!HasKey) // ключа нет
                    {
                        HasKey = true; // "подбираем ключ"
                        inPathObject.Destroy();
                    }
                    break;
                case CannonObject _:
                    if (!HasGun) // пушки нет
                    {
                        Ammo += 3;
                        HasGun = true; // "подбираем пушку"
                        inPathObject.Destroy();
                    }
                    break;
                case AmmoObject _:
                    Ammo += 3;
                    inPathObject.Destroy();
                    break;
                case CoinObject _:
                    Controller.Score++;
                    inPathObject.Destroy();
                    break;
                case ExitDoorObject _:

                    if (Controller.TuxCount == 0)
                    {
                        Controller.ShowBox("Вы выиграли!");
                        Controller.Stop();
                    }
                    else 
                    {
                        Controller.ShowBox("Спасите всех пингвинов!");
                        return false;
                    }

                    break;
            }
            Move(newCoors, true);
            return true;
        }
    }
}