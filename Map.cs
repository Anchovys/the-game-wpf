using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;

namespace the_game_wpf
{
    public class Map
    {
        public int Width;
        public int Height;
        private Dictionary<MyPoint, GameObject> GameObjects = new Dictionary<MyPoint, GameObject>();

        public Map(string textFile) 
		{
            if (!File.Exists(textFile))
                return;

            var lines = File.ReadAllLines(textFile);

            if (lines.Length == 0 || lines[0].Length == 0)
                return; // при ошибке

            Height = lines.Length;
            Width = lines[0].Length;

            try
            {
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        GameObject gameObject = null;
                        switch (lines[y][x])
                        {
                            case WallObject.InitChar:
                                gameObject = new WallObject(new MyPoint(x, y));
                                break;
                            case ExitObject.InitChar:
                                gameObject = new ExitObject(new MyPoint(x, y));
                                break;
                            case CoinObject.InitChar:
                                gameObject = new CoinObject(new MyPoint(x, y));
                                break;
                        }

                        if (gameObject != null)
                        {
                            PlaceObject(gameObject.Position, gameObject);
                            gameObject.MyMap = this;
                        }
                        
                    }
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка при загрузке карты! {0}", e);
            }
        }

        public void Drawing(Canvas parent) 
		{
            parent.Children.Clear();
            foreach (var item in GameObjects)
            {
                GameObject @object = item.Value;
				
				parent.Children.Add(@object.Figure);
                MyPoint position = @object.GetAbsolutePositionByCoordinates(@object.Position);

                Canvas.SetLeft(@object.Figure, position.X);
                Canvas.SetTop(@object.Figure, position.Y);
            }
        }

        public GameObject GetByCoords(MyPoint cords) 
		{
            if (!GameObjects.ContainsKey(cords))
                return null;

            return GameObjects[cords];
        }

        public bool PlaceObject(MyPoint cords, GameObject gameObject, bool replace = false) 
		{
            if (!GameObjects.ContainsKey(cords)) // без замены обьекта
            {
				GameObjects.Add(cords, gameObject);
                return true;
            }
            else if(replace) // заменять обьект, даже если там что-то есть
			{
				GameObjects[cords] = gameObject;
                return true;
            }

            return false;
		}
    }
}
