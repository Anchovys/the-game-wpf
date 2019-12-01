using System;
using System.Collections.Generic;

namespace the_game_wpf
{
    /// <summary>
    /// Здесь описан основной элемент - точка.
    /// Используется для передачи координат повсеместно
    /// </summary>
    public class MyPoint
    {
        public int X;
        public int Y;

        public string String()
        {
            return string.Format("(X:{0}; Y:{1})", X, Y);
        }

        /// <summary>
        /// Находит расстояние между двумя обьектами
        /// </summary>
        /// <returns>Расстояние</returns>
        public int GetDistance(MyPoint pointTwo)
        {
            return (int)Math.Sqrt(Math.Pow(pointTwo.X - X, 2) + Math.Pow(pointTwo.Y - Y, 2));
        }

        /// <summary>
        /// Возвращает все точки, находящиеся в радиусе от текущей точки
        /// </summary>
        /// <param name="zone">Указывает радиус</param>
        /// <returns>Массив всех найденных точек</returns>
        public MyPoint[] GetNearPoints(int zone, bool removeDiag = false)
        {
            var collection = new List<MyPoint>();
            int zoneY = zone;

            if (zone > 0)
                for (int x = zone * -1; x <= zone; x++)
                {
                    for (int y = zone * -1; y <= zoneY; y++)
                    {
                        MyPoint findPoint = new MyPoint() { X = X + x, Y = Y + y };

                        // срезать диагональные углы
                        if (removeDiag && x == -zone && y == -zone || x == -zone && y == zone ||
                            x == zone && y == -zone || x == zone && y == zone)
                            continue;

                        collection.Add(findPoint);
                    }
                    zoneY = zone;
                }

            return collection.ToArray();
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}", X, Y).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MyPoint);
        }

        public bool Equals(MyPoint obj)
        {
            return obj != null && obj.X == X && obj.Y == Y;
        }
    }
    /// <summary>
    /// Переопределим Equals и GetHashCode для правильной работы с POINT
    /// </summary>
    public class MyPointSpecialComparer : IEqualityComparer<MyPoint>
    {
        public bool Equals(MyPoint obj1, MyPoint obj2)
        {
            return obj1 != null && obj2 != null && obj1.X == obj2.X && obj1.Y == obj2.Y;
        }

        public int GetHashCode(MyPoint obj)
        {
            return string.Format("{0}{1}", obj.Y, obj.Y).GetHashCode();
        }
    }
}