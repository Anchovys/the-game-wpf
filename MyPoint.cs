namespace the_game_wpf
{
    public class MyPoint
    {
        public int X;
        public int Y;

        public MyPoint()
        {
            // "заглушка"
        }

        public MyPoint(MyPoint point)
        {
            X = point.X;
            Y = point.Y;
        }

        public MyPoint(int tX, int tY)
        {
            X = tX;
            Y = tY;
        }

        /// <summary>
        /// Метод вывода координат как строки
        /// </summary>
        public string String()
        {
            return string.Format("(X:{0}; Y:{1})", X, Y);
        }
    }
}
