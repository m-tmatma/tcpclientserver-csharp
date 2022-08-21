#define STREAM_READER_WRITER
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace tcpclient_async
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ipString = "127.0.0.1";
            int port = 2001;

            var ipAddr = System.Net.IPAddress.Parse(ipString);
            var ipendpoint = new IPEndPoint(ipAddr, port);

            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                string data = "test";
                var task = SendToServer(ipendpoint, data);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }


        async static Task SendToServer(IPEndPoint ipendpoint, string data)
        {
            var client = new TcpClient();
            await client.ConnectAsync(ipendpoint);

#if STREAM_READER_WRITER
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                stream.ReadTimeout = 1000;
                stream.WriteTimeout = 1000;
                for ( int i = 0; i < 100; i++)
                {
                    await writer.WriteLineAsync(data);
                    await writer.FlushAsync();
                    Console.WriteLine($"WROTE: {data}");
                    string? res = await reader.ReadLineAsync();
                    if (res != null)
                    {
                        Console.WriteLine($"READ: {res}");
                    }
                }
            }
#else
            using (var stream = client.GetStream())
            {
                stream.ReadTimeout = 1000;
                stream.WriteTimeout = 1000;

                byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(data + '\n');
                await stream.WriteAsync(sendBytes);

                byte[] receiveBytes = new byte[1000];
                int size = await stream.ReadAsync(receiveBytes);
                Console.WriteLine($"READ: size = {size}");
            }
#endif
        }
    }
}