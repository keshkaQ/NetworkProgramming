using System.Net.Sockets;
using System.Text;
//using TcpClient tcpClient = new TcpClient();
//var server = "www.google.com";

//// ЖДЕМ подключения
//await tcpClient.ConnectAsync(server, 80); // Google использует порт 80 для HTTP

//var stream = tcpClient.GetStream();

//var requestMessage = $"GET / HTTP/1.1\r\nHost: {server}\r\nConnection: Close\r\n\r\n";
//var requestData = Encoding.UTF8.GetBytes(requestMessage);

//// ЖДЕМ отправки
//await stream.WriteAsync(requestData);

//// буфер для получения данных
//var responseData = new byte[512];
//var response = new StringBuilder();
//int bytes;

//// Читаем данные асинхронно
//do
//{
//    bytes = await stream.ReadAsync(responseData, 0, responseData.Length);
//    response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
//}
//while (bytes > 0);

//Console.WriteLine(response.ToString());


using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
Console.WriteLine("Клиент запущен");
try
{
    await tcpClient.ConnectAsync("127.0.0.1", 8888);
    byte[] data = new byte[512];
    int bytes = await tcpClient.ReceiveAsync(data);
    string time = Encoding.UTF8.GetString(data,0,bytes);
    Console.WriteLine($"Текущее время: {time}");
 
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
