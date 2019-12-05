using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Diagnostics;

namespace the_game_wpf
{
    public class Map
    {
        public int Width;
        public int Height;

        private readonly Dictionary<MyPoint, GameObject> GameObjects = new Dictionary<MyPoint, GameObject>();
        private Dictionary<MyPoint, GameObject> Changes = new Dictionary<MyPoint, GameObject>();
		private readonly Dispatcher Dispatcher;

        public bool LoadStatus;

        public new int GetHashCode()
        {
            StringBuilder str = new StringBuilder();

            foreach (var item in GameObjects)
                str.Append("|" + item.Key.String() + item.Value.GetType().FullName);

            return str.ToString().GetHashCode();
        }

        public Map(string textFile, GameController controller) 
		{

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (!File.Exists(textFile))
                return;

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
                                case ExitObject.InitChar:
                                    gameObject = new ExitObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case CoinObject.InitChar:
                                    gameObject = new CoinObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case HeroObject.InitChar:
                                    gameObject = new HeroObject(new MyPoint() { X = x, Y = y });
                                    break;
                                case EnemyObject.InitChar:
                                    gameObject = new EnemyObject(new MyPoint() { X = x, Y = y });
                                    break;
                            }

                            if (gameObject != null)
                            {
                                PlaceObject(gameObject.Position, gameObject);
                                gameObject.MyMap = this;
                                gameObject.Controller = controller;
                            }

                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Console.WriteLine("Map file read success - {0} ms", sw.ElapsedMilliseconds);
            });

            sw.Stop();
            LoadStatus = true;

        }

        int hash;
        public void Drawing(Canvas parent, bool reset = false) 
		{
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // оптимизация #0 - проверка на изменение карты по хешу
            if (hash != GetHashCode())
            {
                hash = GetHashCode();

                Dispatcher.Invoke(() =>
                {
                    // если указано, что нужно сбросить поле, сбрасываем
                    if (reset)
                    {
                        Console.WriteLine("******************** CLEANING");
                        parent.Children.Clear();
                    }

                    foreach (var item in Changes)
                    {
                        GameObject @object = item.Value;

                        if (@object == null)
                            continue;

                        if (GameObjects.ContainsValue(@object))
                            parent.Children.Remove(@object.Figure);

						parent.Children.Add(@object.Figure);

                        MyPoint position = @object.GetAbsolutePositionByCoordinates(@object.Position);

                        Canvas.SetLeft(@object.Figure, position.X);
                        Canvas.SetTop(@object.Figure, position.Y);
                    }
                });
            }

            Console.WriteLine("Map drawing success - {0} ms\nInfo: [changes: {1} / {2}];",
                sw.ElapsedMilliseconds,
                Changes.Count,
                GameObjects.Count
            );

            sw.Stop();
            Changes.Clear();
        }

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

        public bool PlaceObject(MyPoint cords, GameObject gameObject, bool replace = false)
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
                if (gameObject != null) 
					GameObjects[cords] = gameObject;
                else GameObjects.Remove(cords);
                return true;
            }
            return false;
        }
    }
}