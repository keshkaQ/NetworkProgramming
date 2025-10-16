using Consumer.Models;
using Consumer.Repositories;
using Consumer.Services;
using Consumer.ViewModels;
using System.Windows;

namespace Consumer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация сервисов
            var dbContext = new DBContext();
            dbContext.Database.EnsureCreated();
            var orderRepository = new OrderRepository(dbContext);
            var orderService = new OrderService(orderRepository, new CustomerRepository(dbContext));
            var queueService = new QueueService();

            // Связываем QueueService с OrderService
            queueService.MessageReceived += async (message) =>
            {
                await orderService.ProcessOrderFromMessageAsync(message);
            };
            // Создаем и устанавливаем ViewModel
            var mainViewModel = new MainViewModel(queueService);
            DataContext = mainViewModel;
        }

        protected override async void OnClosed(EventArgs e)
        {
            // Останавливаем очередь при закрытии приложения
            if (DataContext is MainViewModel viewModel && viewModel.IsQueueRunning)
            {
                 viewModel.StopQueueAsync();
            }
            base.OnClosed(e);
        }
    }
}