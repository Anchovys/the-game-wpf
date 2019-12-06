namespace the_game_wpf
{
    class Tick
    {
        public readonly int TickSpeed = 50;         // с какой скоростью увеличивать тики
        public readonly int TickRate = 10;          // через какое количество тиков их нужно сбросить
        int CurTick;                // текущий тик, временное значение
        bool Starts;                // определяет состояние цикла while
        public bool InPause;        // находится ли цикл сейчас на паузе
        public int Iteration = 0;   // текущая итерация (число вызовов Update)

        GameController Controller;

        public Tick(GameController controller, int speed = 10, int rate = 10)
        {
            Controller = controller;
            TickSpeed = speed;
            TickRate = rate;
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

                Controller.Update(CurTick);

                if (CurTick == TickRate)
                    CurTick = 0;

                CurTick++;
                Iteration++;
            }
        }
    }
}
