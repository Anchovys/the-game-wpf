using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;

namespace the_game_wpf
{
    public class Map
    {
        public int Width;
        public int Height;

        private readonly Dictionary<MyPoint, GameObject> GameObjects = new Dictionary<MyPoint, GameObject>();
        private readonly Dictionary<MyPoint, GameObject> Changes = new Dictionary<MyPoint, GameObject>();
        private readonly List<UIElement> ElementsForRemove = new List<UIElement>();

        private readonly Dispatcher Dispatcher;

        public bool LoadStatus;

        /// <summary>
        /// Переписанный метод для хеш-кода
        /// </summary>
        public new int GetHashCode()
        {
            StringBuilder str = new StringBuilder();

            foreach (var item in GameObjects)
            {
                string name = item.Value == null ? "null" : item.Value.GetType().FullName;
                str.Append("|" + item.Key.ToString() + name);
            }

            return str.ToString().GetHashCode();
        }

        public Map(string textFile, GameController controller)
        {
            // необходимо проверить, есть ли карта
            if (!File.Exists(textFile))
                return; // при ошибке

            // читаем карту
            var lines = File.ReadAllLines(textFile);

            if (lines.Length == 0 || lines[0].Length == 0)
                return; // при ошибке

            Height = lines.Length;
            Width = lines[0].Length;

            Dispatcher = controller.Window.Dispatcher;
            Dispatcher.Invoke(() =>
            {
                try
                {
                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject gameObject = null;

                            switch (lines[y][x])
                            {
                                case WallObject.InitChar:
                                    gameObject = new WallObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case ExitDoorObject.InitChar:
                                    gameObject = new ExitDoorObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case CoinObject.InitChar:
                                    gameObject = new CoinObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case HeroObject.InitChar:
                                    gameObject = new HeroObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case DemonObject.InitChar:
                                    gameObject = new DemonObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case TuxObject.InitChar:
                                    gameObject = new TuxObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case ClosedDoorObject.InitChar:
                                    gameObject = new ClosedDoorObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case KeyObject.InitChar:
                                    gameObject = new KeyObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case AmmoObject.InitChar:
                                    gameObject = new AmmoObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case CannonObject.InitChar:
                                    gameObject = new CannonObject(new MyPoint() { X = x, Y = y });
                                    break;
                            }

                            if (gameObject is GameObject)
                            {
                                PlaceObject(gameObject.Position, gameObject);
                                gameObject.MyMap = this;
                                gameObject.Controller = controller;
                                gameObject.BlockSizeInPixelsX = (int)(controller.GameField.Width / Width);
                                gameObject.BlockSizeInPixelsY = (int)(controller.GameField.Height / Height);
                            }

                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(" \n\n\t{0}\n\n", e);
                    LoadStatus = false;
                    return;
                }
               
            });

            LoadStatus = true;

        }

        /// <summary>
        /// Проверяет точку на лимиты в карте
        /// </summary>
        /// <param name="point">Точка</param>
        /// <returns>Принадлежит ли точка карте</returns>
        public bool CheckLimits(MyPoint point)
        {
            return point.X > 0 && point.X < Width-1 && point.Y > 0 && point.Y < Height-1;
        }

        int hash; // последний хеш (для сверки)
        public void Drawing(Canvas parent, bool reset = false) 
		{
            Stopwatch sw = new Stopwatch();
            sw.Stop(); sw.Start();

            // оптимизация #0 - проверка на изменение карты по хешу
            if (hash != GetHashCode())
            {
                hash = GetHashCode();

                Dispatcher.Invoke(() =>
                {
                    // если указано, что нужно сбросить поле, сбрасываем
                    if (reset) parent.Children.Clear();

                    foreach (var item in ElementsForRemove)
                    {
                        parent.Children.Remove(item);
                    }

                    // перебор всех "измененных обьектов"
                    foreach (var item in Changes)
                    {
                        GameObject @object = item.Value; // что за обьект (UI ELEMENT)

                        if (@object == null) 
                        {
                            if(item.Key != null)
                                parent.Children.Remove(GameObjects[item.Key].Figure);
                            continue;
                        }

                        // удалим старую фигуру если она есть
                        // для того, чтобы не возникало проблем с клонами
                        if (GameObjects.ContainsValue(@object))
                            parent.Children.Remove(@object.Figure);

                        // добавим новую
						parent.Children.Add(@object.Figure);

                        // получаем расположение в пикселях по координатам
                        MyPoint position = @object.GetAbsolutePositionByCoordinates(@object.Position);

                        // распологаем обьект
                        Canvas.SetLeft(@object.Figure, position.X);
                        Canvas.SetTop(@object.Figure, position.Y);
                    }
                });
            }

            Console.WriteLine("Map drawing success - {0} ms | Info: [changes: {1} / {2}]; {3}",
                sw.ElapsedMilliseconds, Changes.Count, GameObjects.Count, ElementsForRemove.Count);

            Changes.Clear();
            ElementsForRemove.Clear();
        }

        /// <summary>
        /// Получить обьект по координатам
        /// </summary>
        /// <param name="cords">Координаты</param>
        /// <returns>Обьект или null</returns>
        public GameObject GetByCoords(MyPoint cords) 
		{
            if (!GameObjects.ContainsKey(cords))
                return null;

            return GameObjects[cords];
        }

        /// <summary>
        /// Находит первого попавшегося обьекта
        /// </summary>
        /// <param name="mapObject">Тип обьекта, который нужно найти</param>
        /// <returns>Обьект</returns>
        public GameObject FindObject(GameObject mapObject)
        {
            GameObject result = null;

            foreach (var item in GameObjects)
            {
                GameObject @object = item.Value;
                if (@object != null && @object.GetType() == mapObject.GetType())
                    result = @object;
            }

            return result;
        }

        /// <summary>
        /// Находит всех попавшихся обьектов с определенным типом
        /// </summary>
        /// <param name="mapObject">Тип обьектов, которые нужно найти</param>
        /// <returns>Обьект</returns>
        public GameObject[] FindObjects(GameObject mapObject)
        {
            var result = new List<GameObject>();

            foreach (var item in GameObjects)
            {
                GameObject @object = item.Value;
                if (@object != null && @object.GetType() == mapObject.GetType())
                    result.Add(@object);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Проверить что между координатами имеется стена
        /// </summary>
        /// <param name="pos1">Первая позиция, например игрока</param>
        /// <param name="pos2">Вторая позиция, например монстра</param>
        /// <remarks>Работает только на расстоянии 2 исправно</remarks>
        /// <returns>Есть ли стена / булево</returns>
        public bool WallCheck(MyPoint pos1, MyPoint pos2)
        {
            MyPoint newCords = new MyPoint();

            if (pos1.X > pos2.X)
                newCords.X = pos2.X + 1;
            else newCords.X = pos2.X - 1;

            if (pos1.Y > pos2.Y)
                newCords.Y = pos2.Y + 1;
            else newCords.Y = pos2.Y - 1;

            return GetByCoords(newCords) is WallObject;
        }


        /// <summary>
        /// Находит координаты ближайшего обьекта
        /// </summary>
        /// <param name="mapObjects">Тип обьекта, который нужно найти</param>
        /// <param name="startPoint">Стартовая позиция, от которой нужно мерять расстояние</param>
        /// <param name="points">Позиции на карте, которые нужно перебрать</param>
        /// <returns>Обьект</returns>
        public GameObject FindNearlyObject(GameObject[] mapObjects, MyPoint startPoint, MyPoint[] points)
        {
            GameObject result = null;

            int dist = -1;
            foreach (var item in mapObjects)
                foreach (var point in points)
                {
                    GameObject tObject = GetByCoords(point);
                    if (tObject != null && tObject.GetType() == item.GetType())
                    {
                        int tDist = startPoint.GetDistance(point);
                        if (tDist != 0 && (dist == -1 || tDist < dist))
                        {
                            result = tObject;
                            dist = tDist;
                        }
                    }
                }
            return result;
        }

        /// <summary>
        /// Разместить обьект на карте
        /// </summary>
        /// <param name="cords">Координаты размещения</param>
        /// <param name="gameObject">Что за обьект</param>
        /// <param name="replace">Заменять обьект если на этой позиции уже есть обьект?</param>
        /// <returns>Вернет результат работы</returns>
        public bool PlaceObject(MyPoint cords, GameObject gameObject, bool replace = false)
        {
            Dispatcher.Invoke(() =>
            {
                if (gameObject == null && replace)
                {
                    if (!GameObjects.ContainsKey(cords))
                        return false;

                    ElementsForRemove.Add(GameObjects[cords].Figure);
                    GameObjects.Remove(cords);
                    Changes.Remove(cords);

                    return true;
                }
                else
                {
                    if (Changes.ContainsKey(cords))
                        Changes.Remove(cords);

                    Changes.Add(cords, gameObject);

                    if (!GameObjects.ContainsKey(cords)) // без замены обьекта
                    {
                        GameObjects.Add(cords, gameObject);
                        return true;
                    }
                    else if (replace) // заменять обьект, даже если там что-то есть
                    {
                        GameObjects[cords] = gameObject;
                        return true;
                    }
                }
                return false;
            });
            return false;
        }
    }
}