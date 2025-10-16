using Producer.Models;
using Producer.Repositories;
using Producer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Producer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private iPhoneModel _selectedModel;
        private Iphone _currentiPhone;
        private DBContext _dbContext;
        private readonly RabbitMQService _messageService;
        private readonly IphoneRepository _iPhoneRepository;

        public ObservableCollection<iPhoneModel> iPhoneModels { get; set; } = new ObservableCollection<iPhoneModel>();

        public iPhoneModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                if (_selectedModel != null && _selectedModel.ColorVariants.Any())
                {
                    CurrentiPhone = _selectedModel.ColorVariants.First();
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(FinalPrice));
            }
        }

        public Iphone CurrentiPhone
        {
            get => _currentiPhone;
            set
            {
                _currentiPhone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FinalPrice));
            }
        }

        public string FinalPrice => CurrentiPhone?.Price ?? "0 ₽";

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new DBContext();
            _dbContext.InitializeData();
            _iPhoneRepository = new IphoneRepository(_dbContext);
            _messageService = new RabbitMQService();
            DataContext = this;
            LoadIphoneModel();
            InitializeMessaging();
        }

        private async void InitializeMessaging()
        {
            await _messageService.InitializeAsync();
        }

        private void LoadIphoneModel()
        {
            try
            {
                var models = _iPhoneRepository.LoadIphoneModels();
                iPhoneListBox.ItemsSource = models;

                iPhoneModels.Clear();
                foreach (var model in models)
                {
                    iPhoneModels.Add(model);
                }

                if (iPhoneModels.Any())
                {
                    SelectedModel = iPhoneModels[0];
                    if (SelectedModel.ColorVariants.Any())
                    {
                        ColorListBox.ItemsSource = SelectedModel.ColorVariants;
                        ColorListBox.SelectedIndex = 0;
                    }
                }
                SimListBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void iPhoneListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (iPhoneListBox.SelectedItem is iPhoneModel selected)
            {
                SelectedModel = selected;
                ColorListBox.ItemsSource = selected.ColorVariants;

                if (selected.ColorVariants.Any())
                {
                    ColorListBox.SelectedIndex = 0;
                }
            }
        }

        private void ColorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorListBox.SelectedItem is Iphone selectedColor)
            {
                CurrentiPhone = selectedColor;
            }
        }
        private void CustomerEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string email = textBox.Text;

            if (IsValidEmail(email))
                ErrorTextBlock.Text = "";
            else
                ErrorTextBlock.Text = "Неверный формат email";
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                string pattern = @"^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w+)+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void customerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string name = textBox.Text;
            if (IsValidName(name))
                ErrorNameTextBlock.Text = "";
            else
                ErrorNameTextBlock.Text = "Имя должно состоять только из букв";
        }
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            try
            {
                string pattern = @"^[a-zA-Zа-яА-Я]+$";
                return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private async void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedModel == null || CurrentiPhone == null)
            {
                MessageBox.Show("Пожалуйста, выберите iPhone", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SimListBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип SIM-карты", "Неполные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(customerName.Text) || string.IsNullOrWhiteSpace(customerEmail.Text))
            {
                MessageBox.Show("Пожалуйста, введите Ваше имя и Email", "Неполные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSim = (SimListBox.SelectedItem as ListBoxItem)?.Content.ToString();

            try
            {
                var orderMessage = new OrderMessage
                {
                    ModelName = SelectedModel.ModelName,
                    Color = CurrentiPhone.Color,
                    Storage = CurrentiPhone.Storage, 
                    SimType = selectedSim,
                    CustomerName = customerName.Text,
                    CustomerEmail = customerEmail.Text,
                    FinalPrice = CurrentiPhone.BasePrice,
                    OrderDate = DateTime.Now,
                    BasePrice = CurrentiPhone.BasePrice,
                    ImageSource = CurrentiPhone.ImageSource,
                    Specifications = CurrentiPhone.Specifications
                };

                await _messageService.SendOrderMessageAsync(orderMessage);

                MessageBox.Show($"Заказ отправлен в обработку!\n\n" +
                              $"Модель: {SelectedModel.ModelName}\n" +
                              $"Цвет: {CurrentiPhone.Color}\n" +
                              $"Память: {CurrentiPhone.Storage}\n" +
                              $"SIM: {selectedSim}\n" +
                              $"Итоговая цена: {FinalPrice}",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

