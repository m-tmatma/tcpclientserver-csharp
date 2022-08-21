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
        public void Start(int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            var server = new TcpListener(endpoint);
            server.Start();

            _ = server.AcceptTcpClientAsync();
        }

        async Task AcceptTcpClientAsync(TcpListener server)
        {
            using (var client = await server.AcceptTcpClientAsync())
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                do
                {
                    var message = await reader.ReadLineAsync();
                    string currentTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

                    try
                    {
                        await writer.WriteLineAsync($"[{currentTime}] {message}");
                        await writer.FlushAsync();
                    }
                    catch
                    {
                        break;
                    }
                } while (client.Connected);
            }
        }
    }
}
