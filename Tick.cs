namespace the_game_wpf
{
    class Tick
    {
        int TickSpeed = 10;         // с какой скоростью увеличивать тики
        int TickRate = 30;          // через какое количество тиков их нужно сбросить
        int CurTick;                // текущий тик, временное значение
        bool Starts;                // определяет состояние цикла while
        public bool InPause;        // находится ли цикл сейчас на паузе
        public int Iteration = 0;   // текущая итерация (число вызовов Update)

        GameController Controller;

        public Tick(GameController controller, int speed = 10, int rate = 10)
        {
            Controller = controller;
            TickSpeed = speed;
            CurTick = rate;
        }

        public void Run()
        {
            Starts = true;
            Reset();
            Cycle();
        }

        public void Stop()
        {
            Starts = false;
        }

        public void Reset() 
        {
            CurTick = 0;
            Iteration = 0;
        }

        void Cycle()
        {
            while (Starts)
            {
                System.Threading.Thread.Sleep(TickSpeed);

                if (!InPause)
                    continue;

                if (CurTick == TickRate)
                {
                    Iteration++;
                    CurTick = 0;
                    Controller.Update();
                }
                CurTick++;
            }
        }
    }
}
