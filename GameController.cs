using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace the_game_wpf
{
    public class GameController
    {
        private readonly Map MainMap;               // игровая карта
        private readonly HeroObject HeroObject;     // игрок (чтобы не искать каждый кадр)
        public  readonly MainWindow Window;         // окно основного потока - нужно для изменения
        private readonly Canvas GameCanvas;         // игровое поле, на котором будут распологаться обьекты
        private readonly Tick Ticks;                // управление тиками
        public  readonly GameOptions Options;       // загруженные настройки игры

        public int LastInputKey = -1;

        public GameController(MainWindow window)
        {
            Console.WriteLine("GameController init With options: ");

            // работаем с настройками
            Options = new GameOptions();    // сначала дефолтные
            if (!Options.Pop(Options))      // пытаемся загрузить из файла
            {
                // сохраняем дефолтные настройки в файл
                Options = new GameOptions();  
                Options.Push();
            }

            // вывод настроек на экран
            foreach (var f in Options.GetType().GetFields())
                Console.WriteLine("===> Name: {0} Value: {1}", f.Name, f.GetValue(Options));

            Window = window;
            GameCanvas = Window.GameField;

            Ticks = new Tick(this, Options.TickSpeed, Options.TickPerFrame);
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
        /// Метод завершения работы контроллера
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
        public void Update(int deltatime)
        {
            Console.WriteLine("==== new frame {0} ====", deltatime);
            Stopwatch sw = new Stopwatch();
            sw.Stop(); sw.Start();

            if (deltatime == 1)
            {
                // поиск монстров
                // СЛИШКОМ МЕДЛЕННО ( > 5 ms)!
                foreach (var item in MainMap.FindObjects(new EnemyObject()))
                {
                    EnemyObject enemy = item as EnemyObject;
                    enemy.Move();
                    enemy.CheckCollision();
                }
            }

            if (deltatime == 1 || deltatime == Ticks.TickRate / 2)
            {
                if (LastInputKey != -1) // какая-то кнопка была зажата
                {
                    HeroObject.Move((Key)LastInputKey); // передаем управление игроку
                    LastInputKey = -1; // сбрасываем кнопку
                }
            }

            // отрисовка (на любой первой итерации - очищаем поле)
            MainMap.Drawing(GameCanvas, Ticks.Iteration == 0);

            Console.WriteLine("Frame end -> ~{0} мс", sw.ElapsedMilliseconds, Ticks.Iteration);
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