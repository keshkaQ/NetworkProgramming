using Consumer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace Consumer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly QueueService _queueService;
        private bool _isQueueRunning;
        private string _queueStatus = "Остановлена";
        private Brush _queueStatusColor = Brushes.Red;

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        public ICommand StartQueueCommand { get; }
        public ICommand StopQueueCommand { get; }
        public ICommand ClearLogsCommand { get; }

        public bool IsQueueRunning
        {
            get => _isQueueRunning;
            set
            {
                _isQueueRunning = value;
                OnPropertyChanged();
            }
        }

        public string QueueStatus
        {
            get => _queueStatus;
            set
            {
                _queueStatus = value;
                OnPropertyChanged();
            }
        }

        public Brush QueueStatusColor
        {
            get => _queueStatusColor;
            set
            {
                _queueStatusColor = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(QueueService queueService)
        {
            _queueService = queueService;
            _queueService.MessageReceived += OnMessageReceived;
            _queueService.StatusChanged += OnStatusChanged;

            // Инициализация команд
            StartQueueCommand = new RelayCommand(StartQueueAsync, () => !IsQueueRunning);
            StopQueueCommand = new RelayCommand(StopQueueAsync, () => IsQueueRunning);
            ClearLogsCommand = new RelayCommand(ClearLogs);
        }
        private void StartQueueAsync()
        {
            try
            {
                 _queueService.StartListeningAsync();
                IsQueueRunning = true;
                QueueStatus = "Запущена";
                QueueStatusColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка запуска: {ex.Message}");
            }
        }

        public void  StopQueueAsync()
        {
            try
            {
                _queueService.StopListeningAsync();
                IsQueueRunning = false;
                QueueStatus = "Остановлена";
                QueueStatusColor = Brushes.Red;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка остановки: {ex.Message}");
            }
        }

        private void ClearLogs()
        {
            Logs.Clear();
            LogMessage("🧹 Логи очищены");
        }

        private void OnMessageReceived(string message)
        {
            LogMessage($"📦 {message}");
        }

        private void OnStatusChanged(string status)
        {
            LogMessage($"ℹ️ {status}");
        }

        private void LogMessage(string message)
        {
            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            App.Current.Dispatcher.Invoke(() =>
            {
                Logs.Add(logEntry);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
