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

        // TODO: Идея - размеры блока в зависимости от размера карты
        public const int BlockSizeInPixelsX = 16; // размеры блока в пикселях по X
        public const int BlockSizeInPixelsY = 16; // размеры блока в пикселях по Y

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

        public Rectangle MakeImage(string path)
        {
            // полный путь. папка со спрайтами указывается через настройки
            string fullpath = System.IO.Path.Combine("assets", path) + ".png";

            // нужно проверить, есть ли файл по пути
            if (!System.IO.File.Exists(fullpath))
                throw new Exception("ERROR :: NOT FOUND IMAGE ON PATH: " + fullpath);

            Rectangle image = new Rectangle
            {
                Width = BlockSizeInPixelsX,
                Height = BlockSizeInPixelsY,
                Fill = new ImageBrush  //вся "магия" - здесь. подставим картинку
                {
                    ImageSource = new BitmapImage(new Uri(fullpath, UriKind.Relative))
                },
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
        // временный обьект, сохраняемый при перемещении обьекта
        private GameObject TempMoveObject = null;

        /// <summary>
        /// Убрать обьект с карты
        /// </summary>
        public void Destroy()
        {
            Console.WriteLine("--> Obj {0} has breen removed from map [FORCED] ({1})", GetType().Name, Position.ToString());
            MyMap.PlaceObject(Position, null, true);
        }

        /// <summary>
        /// Переместить обьект куда-то, на другие координаты
        /// </summary>
        /// <param name="newPosition">Куда перемещать</param>
        public MyPoint Move(MyPoint newPosition, bool notForced = false)
        {
            if (!notForced)
            {
                Console.WriteLine("--> Obj {0} moved to ({1} --> {2}) [FORCED]", GetType().Name, Position.ToString(), newPosition.ToString());
                Destroy(); // уничтожим старый обьект
                Position = newPosition;  // поменяем позицию текущего
                MyMap.PlaceObject(newPosition, this, true); // запишем в новую
                return newPosition;
            }
            else
            {
                Destroy(); // уничтожим старый обьект
                Console.WriteLine("--> Obj {0} moved to ({1} --> {2}) [NOT FORCED]", GetType().Name, Position.ToString(), newPosition.ToString());
                if (TempMoveObject != null)
                {
                    TempMoveObject.Position = Position;
                    MyMap.PlaceObject(Position, TempMoveObject, true);
                    Console.WriteLine("==> NOT FORCED FEATURE :: Placed Object '{0}'", TempMoveObject.GetType().FullName);
                    TempMoveObject = null;
                }
                
                // посмотрим что идет дальше
                GameObject next = MyMap.GetByCoords(newPosition);

                // не пустота и не игрок, запишем в буфер (сохраним)
                if (next != null && next.GetType() != typeof(HeroObject))
                {
                    Console.WriteLine("==> NOT FORCED FEATURE :: Saved Object '{0}'", next.GetType().FullName);
                    TempMoveObject = next;
                }

                MyMap.PlaceObject(newPosition, this, true); // запишем в новую

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
            MyMap.PlaceObject(Position, this, replace);
        }
    }
    public class OpenedDoorObject : GameObject
    {
        public const char InitChar = 'd';   // символ, которым изображен этот обьект на карте
        public OpenedDoorObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
    }
    public class ClosedDoorObject : GameObject
    {
        public const char InitChar = '~';   // символ, которым изображен этот обьект на карте
        public ClosedDoorObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
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
    public class DestroyedWallObject : GameObject
    {
        public const char InitChar = 'w';   // символ, которым изображен этот обьект на карте
        public DestroyedWallObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
    }
    public class KeyObject : GameObject
    {
        public const char InitChar = '/';   // символ, которым изображен этот обьект на карте
        public KeyObject(MyPoint startPosition)
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
            Figure = MakeImage(GetType().Name);
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

            if (!MyMap.CheckLimits(newCoors))
            {
                Freeze = true;
                Clean = true;
                return;
            }

            GameObject nextObject = MyMap.GetByCoords(newCoors);

            if (nextObject != null)
                if (nextObject is WallObject)
                {
                    BulletHealth--;
                    nextObject.Destroy();

                    Controller.Window.Dispatcher.Invoke(() =>
                    {
                        var blood = new DestroyedWallObject(newCoors)
                        {
                            MyMap = MyMap,
                            Controller = Controller
                        };

                        blood.Place();
                    });
                    return;
                }
                else if (nextObject is EnemyObject)
                {
                    nextObject.Destroy();

                    Controller.Window.Dispatcher.Invoke(() =>
                    {
                        var blood = new BloodObject(newCoors)
                        {
                            MyMap = MyMap,
                            Controller = Controller
                        };

                        blood.Place();
                    });

                    return;
                }
                else if (nextObject is HeroObject)
                {
                    HeroObject heroObject = nextObject as HeroObject;
                    heroObject.Kill("Вас раздавило пушечным ядром.");
                }
                else if (nextObject is CoinObject || nextObject is ClosedDoorObject)
                {
                    // меняем направление пули :D
                    LeftDirection = !LeftDirection;
                    return;
                }

            Move(newCoors, true);
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
        /// Обработка коллизий для монстра
        /// </summary>
        public void CheckCollision()
        {
            foreach (var item in Position.GetNearPoints(AttackZoneJumping, true))
            {
                GameObject tObject = MyMap.GetByCoords(item);

                if (tObject is HeroObject && !MyMap.WallCheck(Position, item))
                {
                    HeroObject heroObject = tObject as HeroObject;
                    heroObject.Kill("Вы были погрызаны демонами.");
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
            // обьект уже найден
            if (Target != null)
                MoveToTarget(Target.Position);

            // найти самый ближайший обьект (игрока или монстра)
            // TODO: Игрок в приоритете
            GameObject localObject = MyMap.FindNearlyObject(
                new GameObject[] { new EnemyObject(), new HeroObject() },
                Position, Position.GetNearPoints(ViewZone, false));

            // не должен быть текущим обьектом, в случае нахождения Enemy
            if (localObject != this)
                Target = localObject;
        }

    }
    public class BloodObject : GameObject
    {
        public const char InitChar = 'B';   // символ, которым изображен этот обьект на карте
        public BloodObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }
    }
    public class HeroObject : GameObject
    {
        public const char InitChar = '+';   // символ, которым изображен этот обьект на карте
        public bool LeftDirection = false;
        public bool Haskey = false;
        public HeroObject() { }
        public HeroObject(MyPoint startPosition)
        {
            Position = startPosition;
            Figure = MakeImage(GetType().Name);
        }

        /// <summary>
        /// Убивает игрока
        /// </summary>
        public void Kill(string deathReason = "Вы умерли") 
        {
            Controller.Window.Dispatcher.Invoke(() =>
            {
                var blood = new BloodObject(Position)
                {
                    MyMap = MyMap,
                    Controller = Controller
                };

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

                    if (LeftDirection) newCoors.X--;
                    else newCoors.X++;

                    Controller.Window.Dispatcher.Invoke(() =>
                    {
                        BulletObject bullet = new BulletObject(newCoors)
                        {
                            LeftDirection = LeftDirection,
                            Controller = Controller,
                            Position = newCoors,
                            MyMap = MyMap,
                        };
                        bullet.Place();
                    });
                    

                    return true;
                default:
                    return false;
            }

            GameObject inPathObject = MyMap.GetByCoords(newCoors);
            switch (inPathObject)
            {
                case ClosedDoorObject _:
                    // ключ есть
                    if (Haskey)
                    {
                        // испольюзуем ключ
                        Haskey = false;

                        inPathObject.Destroy();
                        Controller.Window.Dispatcher.Invoke(() =>
                        {
                            MyMap.PlaceObject(newCoors, new OpenedDoorObject(newCoors)
                            {
                                Controller = Controller,
                                Position = newCoors,
                                MyMap = MyMap,
                            });
                        });
                        return false;
                    }
                    return false;

                case WallObject _:
                    return false;

                case KeyObject _:
                    if(!Haskey) // ключа нет
                    {
                        Haskey = true; // "подбираем ключ"
                        inPathObject.Destroy();
                    }
                    break;
                case CoinObject _:
                    inPathObject.Destroy();
                    break;
                case ExitObject _:
                    Controller.ShowBox("Вы выиграли!");
                    Controller.Stop();
                    break;
            }
            Move(newCoors, true);
            return true;
        }
    }
}