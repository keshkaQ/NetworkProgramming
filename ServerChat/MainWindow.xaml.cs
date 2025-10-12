using System.Windows;
using System.Windows.Media;

namespace ServerChat
{
    public partial class MainWindow : Window
    {
        private ChatServer serverManager;
        public MainWindow()
        {
            InitializeComponent();
            InitializeServer();
        }
        private void InitializeServer()
        {
            serverManager = new ChatServer();
            serverManager.OnLogMessage += LogMessage;
            DataContext = serverManager;
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(PortTextBox.Text, out int port))
                {
                    serverManager.StartServer(port);
                    UpdateServerStatus(true);
                }
                else
                {
                    MessageBox.Show("Введите корректный номер порта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска сервера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serverManager.StopServer();
                UpdateServerStatus(false);
                LogMessage("⏹ Сервер остановлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки сервера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            LogsTextBox.Clear();
            LogMessage("🧹 Логи очищены");
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
                LogsTextBox.AppendText(logEntry + Environment.NewLine);
                LogsTextBox.ScrollToEnd();
            });
        }

        private void UpdateServerStatus(bool isRunning)
        {
            StartServerButton.IsEnabled = !isRunning;
            StopServerButton.IsEnabled = isRunning;
            PortTextBox.IsEnabled = !isRunning;

            if (isRunning)
            {
                ServerStatusText.Text = "Запущен";
                ServerStatusText.Foreground = Brushes.Green;
            }
            else
            {
                ServerStatusText.Text = "Остановлен";
                ServerStatusText.Foreground = Brushes.Red;
            }
        }
    }
}