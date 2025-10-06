using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClientApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Product> allProducts;
        private ObservableCollection<Product> filteredProducts;
        private TcpClient tcpClient;
        private const string serverIP = "127.0.0.1"; // IP-адрес сервера

        private ImageSource _dishImageSource;

        public ImageSource DishImageSource
        {
            get => _dishImageSource;
            set
            {
                _dishImageSource = value;
                OnPropertyChanged(nameof(DishImageSource));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            InitializeComponent();
            InitializeProducts();
            DataContext = this;
            LoadImage("placeholder.jpg");
        }

        private void InitializeProducts()
        {
            allProducts = new ObservableCollection<Product>(GetDefaultProducts());
            filteredProducts = new ObservableCollection<Product>(allProducts);
            ProductsListBox.ItemsSource = filteredProducts;
        }

        private List<Product> GetDefaultProducts()
        {
            return new List<Product>
            {
                new Product { Name = "Курица", IsSelected = false },
                new Product { Name = "Сыр", IsSelected = false },
                new Product { Name = "Колбаса", IsSelected = false },
                new Product { Name = "Макароны", IsSelected = false },
                new Product { Name = "Салат", IsSelected = false },
                new Product { Name = "Гречка", IsSelected = false },
                new Product { Name = "Фарш", IsSelected = false },
                new Product { Name = "Лук", IsSelected = false },
                new Product { Name = "Морковь", IsSelected = false },
                new Product { Name = "Картофель", IsSelected = false },
                new Product { Name = "Помидоры", IsSelected = false },
                new Product { Name = "Яйца", IsSelected = false },
                new Product { Name = "Молоко", IsSelected = false },
                new Product { Name = "Мука", IsSelected = false },
                new Product { Name = "Сахар", IsSelected = false },
                new Product { Name = "Огурцы", IsSelected = false },
                new Product { Name = "Соль", IsSelected = false },
                new Product { Name = "Перец", IsSelected = false },
                new Product { Name = "Масло", IsSelected = false },
                new Product { Name = "Рис", IsSelected = false }
            };
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            filteredProducts.Clear();
            foreach (var product in allProducts.Where(p => p.Name.ToLower().Contains(searchText)))
            {
                filteredProducts.Add(product);
            }
        }

        private void GetRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранные продукты
                var selectedProducts = allProducts.Where(p => p.IsSelected).Select(p => p.Name).ToList();

                if (selectedProducts.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы один продукт!", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Формируем запрос
                string request = string.Join(",", selectedProducts);

                // Отправляем запрос серверу
                SendRequestToServer(request);
                GetRecipeButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении рецепта: {ex.Message}", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendRequestToServer(string request)
        {
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[102400]; // 100KB
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                DisplayRecipeWithImage(response);
                ProductsListBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка связи с сервером: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                RecipeTextBlock.Text = "Рецепт приготовления:\n\nВыберите продукты и нажмите 'Получить рецепт'.";
                LoadImage("placeholder.jpg");
                DisconnectFromServer();
            }
        }
        private void DisplayRecipeWithImage(string recipeResponse)
        {
            try
            {
                // Разделяем текст рецепта и изображение
                string recipeText = recipeResponse;
                string imageData = null;

                // Ищем метку изображения в ответе
                if (recipeResponse.Contains("🖼IMAGE:"))
                {
                    int imageIndex = recipeResponse.IndexOf("🖼IMAGE:");
                    recipeText = recipeResponse.Substring(0, imageIndex).Trim();
                    imageData = recipeResponse.Substring(imageIndex + "🖼".Length).Trim();
                }

                // Устанавливаем текст рецепта 
                RecipeTextBlock.Text = recipeText;

                // Загружаем изображение
                if (!string.IsNullOrEmpty(imageData))
                {
                    LoadRecipeImageFromResponse(imageData);
                }
                else
                {
                    LoadImage("placeholder.jpg");
                }
            }
            catch (Exception ex)
            {
                RecipeTextBlock.Text = recipeResponse;
                LoadImage("placeholder.jpg");
            }
        }

        private void LoadRecipeImageFromResponse(string imageData)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageData) && imageData.StartsWith("IMAGE:"))
                {
                    // Берем только имя файла после IMAGE:
                    string imageName = imageData.Substring("IMAGE:".Length).Trim();
                    LoadImage(imageName);
                }
                else
                {
                    LoadImage("placeholder.jpg");
                }
            }
            catch (Exception ex)
            {
                LoadImage("placeholder.jpg");
            }
        }


        private void LoadImage(string imageName)
        {
            try
            {
                string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RecipeImages", imageName);

                if (File.Exists(targetPath))
                {
                    var bitmap = new BitmapImage(new Uri(targetPath));
                    bitmap.Freeze();
                    DishImageSource = bitmap;
                }
                else
                {
                    DishImageSource = null;
                }
            }
            catch
            {
                DishImageSource = null;
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var product in allProducts)
                {
                    product.IsSelected = false;
                }

                ProductsListBox.Items.Refresh();

                LoadImage("placeholder.jpg");
                SearchTextBox.Clear();
                RecipeTextBlock.Text = "Рецепт приготовления:\n\nВыберите продукты и нажмите 'Получить рецепт'.";
                GetRecipeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer();
        }

        private void DisconnectFromServer()
        {
            try
            {
                if(tcpClient != null)
                {
                    try
                    {
                        NetworkStream networkStream = tcpClient.GetStream();
                        string disconnectMsg = "DISCONNECT";
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(disconnectMsg);
                        networkStream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось отправить DISCONNECT: {ex.Message}");
                    }
                    tcpClient.Close();
                    tcpClient = null;
                    UpdateServerStatus(false);
                    MessageBox.Show("Отключено от сервера!", "Информация",MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка при отключении: {ex.Message}", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            int port = default;
            try
            {
                if (!string.IsNullOrWhiteSpace(PortTextBox.Text))
                {
                    if (int.TryParse(PortTextBox.Text, out int newPort))
                    {
                        port = newPort;
                    }
                    else
                    {
                        MessageBox.Show("Введите корректный номер порта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                tcpClient = new TcpClient();
                tcpClient.Connect(serverIP, port);
                UpdateServerStatus(true);
                MessageBox.Show($"Успешно подключено к серверу {serverIP}:{port}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к серверу {serverIP}:{port}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateServerStatus(false);
            }
        }
        private void UpdateServerStatus(bool isRunning)
        {
            StartServerButton.IsEnabled = !isRunning;
            StopServerButton.IsEnabled = isRunning;
            GetRecipeButton.IsEnabled = isRunning;
            ResetButton.IsEnabled = isRunning;
            PortTextBox.IsEnabled = !isRunning;

            if (isRunning)
            {
                ConnectionStatusText.Text = "Подключено к серверу";
                ConnectionStatusText.Foreground = Brushes.Green;
                ellipse.Fill = Brushes.Green;
                ConnectionStatusBorder.BorderBrush = Brushes.Green;
                ConnectionStatusBorder.Background = Brushes.White;
            }
            else
            {
                ConnectionStatusText.Text = "Отключено от сервера";
                ConnectionStatusText.Foreground = Brushes.Red;
                ellipse.Fill = Brushes.Red;
                ConnectionStatusBorder.BorderBrush = Brushes.Red;
                ConnectionStatusBorder.Background = Brushes.White;
            }
        }
    }
}