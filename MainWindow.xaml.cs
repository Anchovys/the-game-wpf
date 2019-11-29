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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public Canvas parentZone;
        GameController controller = new GameController();

        Dispatcher dispatcher;

        public MainWindow()
        {
            InitializeComponent();

            parentZone = GameField;
            dispatcher = Dispatcher;

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int tick = 0;

            while (true)
            {
                System.Threading.Thread.Sleep(10);
                if (tick == 100)
                {
                    
                    tick = 0;

                    controller.mainMap.Drawing(parentZone, dispatcher);
                    controller.heroObject.Move(Key.S);
                    
                }
                else tick++;


            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}
