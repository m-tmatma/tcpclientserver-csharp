namespace tcpserver_async
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = 2001;
            var tcpserver = new TcpServer();
            tcpserver.Start(port);


            Console.WriteLine("Press key to exit.");
            Console.ReadLine();
        }
    }
}