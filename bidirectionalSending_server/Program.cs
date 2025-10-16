using System.Net;
using System.Net.Sockets;
using System.Text;

using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
var words = new Dictionary<string, string>
{
    {"red", "красный" },
    {"blue", "синий" },
    {"green", "зеленый" },
};
try
{
    tcpListener.Bind(new IPEndPoint(IPAddress.Any, 8888));
    tcpListener.Listen();
    Console.WriteLine("Сервер запущен");

    while (true)
    {
        using var tcpClient = await tcpListener.AcceptAsync();
        var response = new List<byte>();
        var bytesRead = new byte[1];
        while (true)
        {
            while (true)
            {
                var count = tcpListener.Receive(bytesRead);
                if (count == 0 || bytesRead[0] == '\n') break;
                response.Add(bytesRead[0]);
            }
            var word = Encoding.UTF8.GetString(response.ToArray());
            if (word == "END") break;
            Console.WriteLine($"Запрошен перевод слова {word}");
            if (!words.TryGetValue(word, out var translation)) translation = "не найдено в словаре";
            translation += '\n';
            await tcpClient.SendAsync(Encoding.UTF8.GetBytes(translation));
            response.Clear();
        }

    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
