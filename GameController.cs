using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace the_game_wpf
{
    public class GameController
    {
        private readonly Map MainMap;
        private readonly HeroObject HeroObject;
        public readonly MainWindow Window;
        private readonly Canvas GameCanvas;
        private readonly Tick tick;

        public int LastInputKey = -1;

        public GameController(MainWindow window)
        {
            tick = new Tick(this);
            Window = window;
            GameCanvas = Window.GameField;

            MainMap = new Map("map.txt", this);
            HeroObject = (HeroObject)MainMap.FindObject(new HeroObject());
        }

        /// <summary>
        /// Метод будет вызван после иницилизации класса
        /// </summary>
        public void Start()
        {
            tick.Run();
        }

        /// <summary>
        /// Метод будет вызываться каждый кадр
        /// </summary>
        public void Update()
        {
            foreach (var item in MainMap.FindObjects(new EnemyObject()))
            {
                EnemyObject enemy = (EnemyObject)item;
                enemy.Move();
                enemy.CheckCollision();
            }

            if (LastInputKey != -1) // какая-то кнопка была зажата
            {
                HeroObject.Move((Key)LastInputKey); // передаем управление игроку
                LastInputKey = -1; // сбрасываем кнопку
            }
            MainMap.Drawing(GameCanvas);
        }

        public void ChangeState(bool to) 
        {
            Console.WriteLine("Состояние " + to);
            tick.InPause = to;
        }

    }
}