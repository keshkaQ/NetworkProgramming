using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // создание
        IPAddress iPAddress = new IPAddress([127, 0, 0, 1]);

        // Свойства
        Console.WriteLine(iPAddress.AddressFamily); // InterNetwork
        Console.WriteLine(iPAddress.GetType());     // System.Net.IPAddress

        // статические  свойства
        Console.WriteLine(IPAddress.Any); // 0.0.0.0
        Console.WriteLine(IPAddress.Loopback); // 127.0.0.1
        Console.WriteLine(IPAddress.Broadcast); // 255.255.255.255

        // Конечные точки - ip + порт
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(ip, 8080);
        Console.WriteLine(endPoint); // 127.0.0.1:8080
        Console.WriteLine(endPoint.Address); // 127.0.0.1
        Console.WriteLine(endPoint.Port); // 8080
        Console.WriteLine(endPoint.AddressFamily); // InterNetwork

        // DNS
        IPHostEntry goodleEntry = await Dns.GetHostEntryAsync("google.com");
        Console.WriteLine(goodleEntry.HostName);
        foreach (var _ip in goodleEntry.AddressList)
            Console.WriteLine(_ip);

        // Информация о сетевых интерфейсах на текущей машине
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in adapters)
        {
            Console.WriteLine("=====================================================================");
            Console.WriteLine();
            Console.WriteLine($"ID устройства: ------------- {adapter.Id}");
            Console.WriteLine($"Имя устройства: ------------ {adapter.Name}");
            Console.WriteLine($"Описание: ------------------ {adapter.Description}");
            Console.WriteLine($"Тип интерфейса: ------------ {adapter.NetworkInterfaceType}");
            Console.WriteLine($"Физический адрес: ---------- {adapter.GetPhysicalAddress()}");
            Console.WriteLine($"Статус: -------------------- {adapter.OperationalStatus}");
            Console.WriteLine($"Скорость: ------------------ {adapter.Speed}");

            IPInterfaceStatistics stats = adapter.GetIPStatistics();
            Console.WriteLine($"Получено: ----------------- {stats.BytesReceived}");
            Console.WriteLine($"Отправлено: --------------- {stats.BytesSent}");
        }

        // Детальная информация по сетевому трафику и его конфигурации \var ipProps = IPGlobalProperties.GetIPGlobalProperties();
        var ipProps = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnections = ipProps.GetActiveTcpConnections();

        Console.WriteLine($"Всего {tcpConnections.Length} активных TCP-подключений");
        Console.WriteLine();
        foreach (var connection in tcpConnections)
        {
            Console.WriteLine("=============================================");
            Console.WriteLine($"Локальный адрес: {connection.LocalEndPoint.Address}:{connection.LocalEndPoint.Port}");
            Console.WriteLine($"Адрес удаленного хоста: {connection.RemoteEndPoint.Address}:{connection.RemoteEndPoint.Port}");
            Console.WriteLine($"Состояние подключения: {connection.State}");
        }
    }
}