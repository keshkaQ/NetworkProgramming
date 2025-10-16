using System.Net.Sockets;
using System.Text;

using Socket tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
var words = new string[] { "red", "yellow", "blue" };

try
{
    tcpClient.Connect("127.0.0.1", 8888);
    var response = new List<byte>();
    foreach(var word in words)
    {
        byte[] data = Encoding.UTF8.GetBytes(word + "\n");
        await tcpClient.SendAsync(data);

        var bytesRead = new byte[1];
        while(true)
        {
            var count = tcpClient.Receive(bytesRead);
            if (count == 0 || bytesRead[0] == '\n') break;
            response.Add(bytesRead[0]);
        }
        var translation = Encoding.UTF8.GetString(response.ToArray());
        Console.WriteLine($"Слово {word}: {translation} ");
    }
    await tcpClient.SendAsync(Encoding.UTF8.GetBytes("END\n"));
    Console.WriteLine("Все сообщения отправлены");
}
catch(SocketException ex)
{ Console.WriteLine(ex.Message); }
