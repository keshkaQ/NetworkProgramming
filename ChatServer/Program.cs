using System;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new ChatServer("127.0.0.1", 8080);
            await server.StartAsync();
        }
    }
}
