using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace the_game_wpf
{
    public class GameController
    {
        public readonly Map MainMap;                // игровая карта
        private readonly HeroObject HeroObject;     // игрок (чтобы не искать каждый кадр)
        public readonly MainWindow Window;          // окно основного потока - нужно для изменения
        public readonly Canvas GameField;           // игровое поле, на котором будут распологаться обьекты
        private readonly Tick Ticks;                // управление тиками
        public readonly GameOptions Options;        // загруженные настройки игры
        private int tuxcount = 0;                   // количество пингвинов
        private int score = 0;

        public int Score // свойство очков
        {
            get {  return score;  }
            set 
            {
                score = value; // обновляем значение
                UpdateScoreTrigger(score); // триггерим обновление
            }
        }

        public int TuxCount // свойство очков
        {
            get { return tuxcount; }
            set
            {
                if (tuxcount != value)
                {
                    tuxcount = value; // обновляем значение
                    UpdateTuxCountTrigger(tuxcount); // триггерим обновление
                }
            }
        }


        public int LastInputKey = -1;

        public GameController(MainWindow window)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            Console.WriteLine("▀ ▀ Init Options... Please, wait!");

            // работаем с настройками
            Options = new GameOptions();    // сначала дефолтные
            if (!Options.Pop(Options))      // пытаемся загрузить из файла
            {
                // сохраняем дефолтные настройки в файл
                Options = new GameOptions();
                Options.Push();
            }

            sw.Stop();
            Console.WriteLine("Options loaded in '{0}'ms", sw.ElapsedMilliseconds);

            Console.WriteLine("GameController init With options: ");

            // вывод настроек на экран
            foreach (var f in Options.GetType().GetFields())
                Console.WriteLine("= Option => Name: {0} Value: {1}", f.Name, f.GetValue(Options));

            // окно передается из аргументов
            Window = window;

            // игровое поле берем из окна
            GameField = Window.GameCanvas;

            // иницилизация тиков
            Ticks = new Tick(this, Options.TickSpeed, Options.TickPerFrame);

            sw.Start();
            Console.WriteLine("Reading map... Please, wait!");

            // иницилизация карты
            MainMap = new Map(this);

            // карта по каким-то причинам не загружена
            if (!MainMap.Load())
                throw new Exception("Map is not loaded!");

            sw.Stop();
            Console.WriteLine("Map read && init at '{0}'ms", sw.ElapsedMilliseconds);

            

            // ищем игрока
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
        /// Метод будет вызываться каждый кадр
        /// </summary>
        public void Update(int deltatime)
        {
            Stopwatch sw = new Stopwatch();
            sw.Stop(); sw.Start();

            if (deltatime == 1 || deltatime == Ticks.TickRate / 2 || deltatime == Ticks.TickRate / 3)
            {
                // поиск всех пуль
                foreach (var item in MainMap.FindObjects(new BulletObject()))
                {
                    BulletObject enemy = item as BulletObject;
                    enemy.Move();  // перемещение пули
                }

                if (deltatime == 1 || deltatime == Ticks.TickRate / 2)
                {
                    if (LastInputKey != -1) // какая-то кнопка была зажата
                    {
                        HeroObject.Move((Key)LastInputKey); // передаем управление игроку
                        LastInputKey = -1; // сбрасываем кнопку
                    }

                    if (deltatime == 1)
                    {
                        // поиск всех демонов
                        foreach (var item in MainMap.FindObjects(new DemonObject()))
                        {
                            DemonObject demon = item as DemonObject;
                            demon.Move();             // движение демона
                            demon.CheckCollision();   // проверка на наличие игрока для сьедания
                        }

                        // найти всех пингвинов
                        GameObject[] gameObjectsTux = MainMap.FindObjects(new TuxObject());

                        // обновим количество пингинов
                        TuxCount = gameObjectsTux.Length;

                        // перебор всех пингвинов
                        foreach (var item in gameObjectsTux)
                        {
                            TuxObject tux = item as TuxObject;
                            tux.Move(); // движение пингвина
                        }
                    }

                }
            }

            // отрисовка (на любой первой итерации - очищаем поле)
            MainMap.Drawing(GameField, Ticks.Iteration == 0);
            Console.WriteLine("$ Frame end -> ~ '{0}'ms on Tick: '{1}'", sw.ElapsedMilliseconds, Ticks.Iteration, deltatime);
        }

        /// <summary>
        /// Метод для установки текущего состояния паузы
        /// </summary>
        /// <returns>Состояние паузы (после изменения)</returns>
        public bool PauseTrigger()
        {
            Ticks.InPause = !Ticks.InPause;
            
            // должны вызвать из диспетчера окна
            Window.Dispatcher.Invoke(() =>
            {
                Window.StartOrPause.Content = Ticks.InPause ? "Остановить" : "Продолжить";
            });

            Console.WriteLine("State changed.. Current -> {0}", Ticks.InPause);

            return Ticks.InPause;
        }

        /// <summary>
        /// Триггер на обновление очков
        /// </summary>
        public void UpdateScoreTrigger(int to) 
        {
            // должны вызвать из диспетчера окна
            Window.Dispatcher.Invoke(() =>
            {
                Window.Scores.Content = to.ToString();
            });
        }

        /// <summary>
        /// Триггер на обновление количества пингвинов
        /// </summary>
        public void UpdateTuxCountTrigger(int to)
        {
            // должны вызвать из диспетчера окна
            Window.Dispatcher.Invoke(() =>
            {
                Window.TuxCounts.Content = to.ToString();
            });
        }

        /// <summary>
        /// Показать какое-то диалоговое окно 
        /// </summary>
        /// <param name="text">Текст диалогового окна</param>
        /// <param name="exit">Выходить из программы после показа</param>
        public void ShowBox(string text, bool exit = false)
        {
            // должны вызвать из диспетчера окна
            Window.Dispatcher.Invoke(() =>
            {
                // вызов в фоне
                Window.Dispatcher.BeginInvoke((Action)(() => MessageBox.Show(text, "Внимание")));

                if (exit) Environment.Exit(0);
            });
        }
    }
}