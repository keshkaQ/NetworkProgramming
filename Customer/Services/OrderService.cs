using Consumer.Models;
using Consumer.Repositories;
using System.Text.Json;
using System.Windows;

namespace Consumer.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly CustomerRepository _customerRepository;
        public OrderService(OrderRepository orderRepository, CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }
        public async Task<bool> ProcessOrderFromMessageAsync(string messageJson)
        {
            try
            {
                var orderData = JsonSerializer.Deserialize<OrderMessage>(messageJson);
                if (orderData == null) return false;

                var customer = _customerRepository.GetOrCreateCustomer(orderData.CustomerName, orderData.CustomerEmail);
                var order = _orderRepository.CreateOrder(orderData.ModelName, orderData.Color, orderData.Storage, orderData.SimType,
                    $"{orderData.FinalPrice:N0} ₽", orderData.OrderDate, customer.Id);
                _orderRepository.SaveOrder(order);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Заказ успешно создан!\nНомер заказа: #{order.Id}",
                                  "Заказ создан", MessageBoxButton.OK, MessageBoxImage.Information);
                });

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return false;
            }
        }
    }
}