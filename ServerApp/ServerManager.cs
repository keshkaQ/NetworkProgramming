using ServerApp.Database;
using ServerApp.Models;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    public class ServerManager : INotifyPropertyChanged
    {
        private TcpListener listener;
        private bool isRunning;
        private List<TcpClient> connectedClients;
        private readonly object _clientsLock = new object();
        private int _connectedClientsCount;
        private RecipeDatabase _recipeDatabase;

        public int ConnectedClientsCount
        {
            get =>_connectedClientsCount; 
            set
            {
                if (_connectedClientsCount != value)
                {
                    _connectedClientsCount = value;
                    OnPropertyChanged(nameof(ConnectedClientsCount));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<string> OnLogMessage;

        public ServerManager()
        {
            connectedClients = new List<TcpClient>();
            ConnectedClientsCount = 0;
            _recipeDatabase = new RecipeDatabase();
        }

        public void StartServer(int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;
                ConnectedClientsCount = 0;

                OnLogMessage?.Invoke($"Сервер запущен на порту {port}");

                Task.Run(() => ListenForClients());
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка запуска сервера: {ex.Message}");
                throw;
            }
        }

        public void StopServer()
        {
            isRunning = false;
            listener?.Stop();

            lock (_clientsLock)
            {
                foreach (var client in connectedClients)
                {
                    client.Close();
                }
                connectedClients.Clear();
                ConnectedClientsCount = 0;
            }
        }

        private async Task ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();

                    lock (_clientsLock)
                    {
                        connectedClients.Add(client);
                        ConnectedClientsCount = connectedClients.Count;
                    }

                    OnLogMessage?.Invoke($"Новое подключение: {GetClientInfo(client)}");
                    Task.Run(() => HandleClient(client));
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"Ошибка при принятии подключения: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            string clientInfo = GetClientInfo(client);
            try
            {
                OnLogMessage?.Invoke($"Обработка клиента: {GetClientInfo(client)}");
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[2048];
                while (client.Connected && isRunning)
                {
                    int bytesRead = await stream.ReadAsync(data, 0, data.Length);
                    if (bytesRead == 0) 
                        break;

                    string request = Encoding.UTF8.GetString(data, 0, bytesRead);
                    OnLogMessage?.Invoke($"Получен запрос от {clientInfo}: {request}");

                    if (request.Trim().ToUpper() == "DISCONNECT")
                    {
                        OnLogMessage?.Invoke($"Клиент {clientInfo} запросил отключение");
                        break;
                    }
                    // Обрабатываем запрос и формируем ответ
                    string response = ProcessRecipeRequest(request);

                    // Отправляем ответ
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    OnLogMessage?.Invoke($"Отправлен ответ клиенту {clientInfo}");

                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка обработки клиента: {ex.Message}");
            }
            finally
            {
                // Удаляем клиента из списка
                lock (_clientsLock)
                {
                    connectedClients.Remove(client);
                    ConnectedClientsCount = connectedClients.Count;
                }

                client.Close();
                OnLogMessage?.Invoke($"Клиент отключен: {clientInfo}");
            }
        }
        private string ProcessRecipeRequest(string request)
        {
            try
            {
                var requestedProducts = request.Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                if (requestedProducts.Count == 0)
                    return "Выберите продукты для получения рецепта!";

                // Ищем рецепты, которые содержат ВСЕ выбранные продукты
                var recipes = _recipeDatabase.FindRecipesByProducts(requestedProducts);

                if (recipes.Count == 0)
                {
                    // Если не нашли рецептов со всеми продуктами, ищем рецепты с любыми из выбранных
                    recipes = _recipeDatabase.FindRecipesByAnyProducts(requestedProducts);

                    if (recipes.Count == 0)
                    {
                        return $"К сожалению, не найдено рецептов для продуктов: {string.Join(", ", requestedProducts)}";
                    }

                    return FormatRecipesResponse(recipes, $"Найдены рецепты, содержащие некоторые из выбранных продуктов ({string.Join(", ", requestedProducts)}):");
                }

                return FormatRecipesResponse(recipes, $"Найдены рецепты для продуктов: {string.Join(", ", requestedProducts)}");
            }
            catch (Exception ex)
            {
                return $"Ошибка при поиске рецептов: {ex.Message}";
            }
        }
        private string FormatRecipesResponse(List<Recipe> recipes, string header)
        {
            var response = new StringBuilder();
            response.AppendLine(header);
            response.AppendLine();

            var recipe = recipes.FirstOrDefault();

            if (recipe != null)
            {
                response.AppendLine($"🍽 {recipe.Name}");
                response.AppendLine($"📝 {recipe.Description}");
                response.AppendLine("📋 Инструкция:");
                response.AppendLine(recipe.Instructions);
                response.AppendLine();

                if (!string.IsNullOrEmpty(recipe.ImagePath))
                {
                    response.AppendLine($"🖼IMAGE:{Path.GetFileName(recipe.ImagePath)}"); 
                }
                else
                {
                    response.AppendLine($"🖼IMAGE:placeholder.jpg");
                }
            }

            return response.ToString();
        }

        private string GetClientInfo(TcpClient client)
        {
            var endPoint = client.Client.RemoteEndPoint as IPEndPoint;
            return endPoint?.Address?.ToString() ?? "неизвестный клиент";
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}