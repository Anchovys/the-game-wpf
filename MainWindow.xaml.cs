using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace the_game_wpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameController controller;
        private readonly BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();

            // иницилизация "Worker"
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                // иницилизация контроллера
                controller = new GameController(this);

                // запуск контроллера 
                controller.Start();
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            // при нажатии кнопки, передаем номер нажатой кнопки в контроллер
            controller.LastInputKey = (int)e.Key;
        }

        private void StartOrPause_Click(object sender, RoutedEventArgs e)
        {
            bool state = controller.Pause();
            StartOrPause.Content = state ? "Остановить" : "Продолжить";
        }

        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigEditor.Text = controller.Options.GetText();
            SaveConfig.IsEnabled = true;
            ConfigEditor.IsEnabled = true;
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig.IsEnabled = false;
            ConfigEditor.IsEnabled = false;
            File.WriteAllText(GameOptions.NormalConfigFile, ConfigEditor.Text);
            MessageBox.Show("Изменения вступят в силу после перезапуска программы!");
            ConfigEditor.Text = "";
        }
    }
}
