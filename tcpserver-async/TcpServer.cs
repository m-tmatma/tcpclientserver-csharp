using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace tcpserver_async
{
    internal class TcpServer
    {
        const int MaxConnection = 18;

        public void Start(int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            var server = new TcpListener(endpoint);
            server.Start();

            // https://stackoverflow.com/questions/55828932/accepttcpclientasync-lagging-on-multiply-connections

            Console.WriteLine("Start Listening");
            var sem = new SemaphoreSlim(MaxConnection, MaxConnection);
            while (true)
            {
                sem.Wait();

                server.AcceptTcpClientAsync().ContinueWith(async task => {
                    Console.WriteLine("Client accepted");

                    var client = task.Result;
                    using (var stream = client.GetStream())
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        do
                        {
                            var message = await reader.ReadLineAsync();
                            Console.WriteLine($"READ: {message}");

                            try
                            {
                                var currentTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                                var res = $"[{currentTime}] {message}";
                                await writer.WriteLineAsync(res);
                                await writer.FlushAsync();
                                Console.WriteLine($"WRITE: {res}");
                            }
                            catch
                            {
                                break;
                            }
                        } while (client.Connected);
                    }

                    sem.Release();
                });
            }
        }
    }
}
