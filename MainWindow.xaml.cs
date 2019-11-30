using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace the_game_wpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameController controller;
        private readonly BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();

            // иницилизация контроллера
            controller = new GameController(this);

            // иницилизация "Worker"
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // запуск контроллера 
            controller.Start();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            // при нажатии кнопки, передаем номер нажатой кнопки в контроллер
            controller.LastInputKey = (int)e.Key;
        }

        bool state = false;
        private void StartOrPause_Click(object sender, RoutedEventArgs e)
        {
            state = !state;

            if (state)
                StartOrPause.Content = "Остановить";
            else
                StartOrPause.Content = "Продолжить";

            controller.ChangeState(state);
            
        }
    }
}
