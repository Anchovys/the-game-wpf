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
using System.Diagnostics;

namespace the_game_wpf
{
    public class GameController
    {
        private readonly Map MainMap;
        private readonly HeroObject HeroObject;
        public  readonly MainWindow Window;
        public  readonly Stopwatch MainTimer;
        private readonly Canvas GameCanvas;
        private readonly Tick Ticks;
        public  readonly GameOptions Options;

        public int LastInputKey = -1;

        public GameController(MainWindow window)
        {
            Console.WriteLine("GameController init With options: ");

            Options = new GameOptions();

            if (!Options.Pop(Options))
            {
                Options = new GameOptions();
                Options.Push();
            }

            foreach (var f in Options.GetType().GetFields())
                Console.WriteLine("===> Name: {0} Value: {1}", f.Name, f.GetValue(Options));


            MainTimer = new Stopwatch();
            MainTimer.Start();

            Ticks = new Tick(this, Options.TickPerFrame, Options.TickSpeed);
            Window = window;
            GameCanvas = Window.GameField;

            MainMap = new Map(Options.MapFilePath, this);

            if (!MainMap.LoadStatus)
                throw new Exception("Map is not loaded!");

            HeroObject = MainMap.FindObject(new HeroObject()) as HeroObject;
        }

        /// <summary>
        /// Метод будет вызван после иницилизации класса
        /// </summary>
        public void Start()
        {
            Ticks.Run();
        }

        /// <summary>
        /// При остановке будет вызван этот метод
        /// </summary>
        public void Stop() 
        {
            Ticks.Stop();
        }

        /// <summary>
        /// Метод для установки текущего состояния паузы
        /// </summary>
        /// <returns>Состояние паузы (после изменения)</returns>
        public bool Pause()
        {
            Ticks.InPause = !Ticks.InPause;
            Console.WriteLine("State changed.. Current -> {0}", Ticks.InPause);
            return Ticks.InPause;
        }

        /// <summary>
        /// Метод будет вызываться каждый кадр
        /// </summary>
        public void Update()
        {
            Console.WriteLine("==== new frame ====");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (LastInputKey != -1) // какая-то кнопка была зажата
            {
                HeroObject.Move((Key)LastInputKey); // передаем управление игроку
                LastInputKey = -1; // сбрасываем кнопку
            }

            // поиск монстров
            // СЛИШКОМ МЕДЛЕННО!
            foreach (var item in MainMap.FindObjects(new EnemyObject()))
            {
                EnemyObject enemy = item as EnemyObject;
                enemy.Move();
                enemy.CheckCollision();
            }

            // отрисовка (на любой первой итерации - очищаем поле)
            MainMap.Drawing(GameCanvas, Ticks.Iteration == 1);

            Console.WriteLine("Frame end -> ~{0} мс", sw.ElapsedMilliseconds, Ticks.Iteration);
            sw.Stop();

        }

        public void ShowBox(string text, bool exit = false)
        {
            // должны вызвать из диспетчера окна
            Window.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(text, "Внимание");
                if (exit) Environment.Exit(0);
            });
        }

    }
}