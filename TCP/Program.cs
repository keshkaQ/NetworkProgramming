using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main()
    {
        // создание tcp на сокетах
        int port = 8080;
        string url = "www.google.com";
        string ip = "127.0.0.1";
        using Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await tcpSocket.ConnectAsync("127.0.0.1", 8888);
            Console.WriteLine($"Подключение к {tcpSocket.RemoteEndPoint} установлено");

            //Console.WriteLine($"Адрес подключения {tcpSocket.RemoteEndPoint}");
            //Console.WriteLine($"Адрес приложения {tcpSocket.LocalEndPoint}");

            //// определяем отправляемые данные
            //var message = $"GET / HTTP/1.1\r\nHost: {url}\r\nConnection: close\r\n\r\n";
            //// конвертируем данные в массив байтов
            //var messageBytes = Encoding.UTF8.GetBytes(message);
            //int bytesSent = await tcpSocket.SendAsync(messageBytes);
            //Console.WriteLine($"на адрес {url} отправлено {bytesSent} байт(а)");

            //// буфер для получения данных
            //var responseBytes = new byte[512];
            //// получаем данные
            //var bytes = await tcpSocket.ReceiveAsync(responseBytes);
            //// преобразуем полученные данные в строку
            //string response = Encoding.UTF8.GetString(responseBytes, 0, bytes);
            //// выводим данные на консоль
            //Console.WriteLine(response);

        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
