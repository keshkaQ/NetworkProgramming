using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        using var mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var server = "www.google.com";
        mySocket.Connect(server, 80);

        using var stream = new NetworkStream(mySocket);
        // отправляем сообщение для отправки
        var message = $"GET / HTTP/1.1\r\nHost: {server}\r\nConnection: Close\r\n\r\n";
        // кодируем его в массив байт
        var data = Encoding.UTF8.GetBytes(message);
        // отправляем массив байт на сервер 
        stream.WriteAsync(data);

        // буфер для получения данных
        var responseData = new byte[512];
        // получаем данные
        var bytes = stream.Read(responseData);
        // преобразуем полученные данные в строку
        string response = Encoding.UTF8.GetString(responseData, 0, bytes);
        // выводим данные на консоль
        Console.WriteLine(response);
    }
}