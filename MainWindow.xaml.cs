using System;
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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dispatcher dispatcher;
        public int tick = 0;
        Map map = new Map("map.txt");
        Canvas parentZone;
        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher;
            parentZone = GameField;

            Load();
        }

        private async void Load()
        {
            await TickTack();
        }

        private async Task TickTack()
        {
            while (true)
            {
                if (tick == 10)
                {
                    map.Drawing(parentZone);
                    tick = 0;
                }
                else tick++;
                await Task.Delay(10); // ~ 100 FPS
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    Console.WriteLine("w");
                    break;
                case Key.D:
                    Console.WriteLine("d");
                    break;
                case Key.A:
                    Console.WriteLine("a");
                    break;
                case Key.S:
                    Console.WriteLine("s");
                    break;
            }
        }
    }
}
