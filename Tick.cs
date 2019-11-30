namespace the_game_wpf
{
    class Tick
    {
        int TickSpeed = 10; // с какой скоростью увеличивать тики
        int TickRate = 50;  // через какое количество тиков их нужно сбросить
        int CurTick;
        bool Starts;
        public bool InPause;
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
            Cycle();
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
                    CurTick = 0;
                    Controller.Update();
                }
                CurTick++;
                
            }
        }
    }
}
