namespace tcpclient
{
    internal class Program
    {
        /// <summary>
        /// https://dobon.net/vb/dotnet/internet/tcpclientserver.html
        /// </summary>
        static void Main(string[] args)
        {
            var FriendlyName = System.AppDomain.CurrentDomain.FriendlyName;
            if (args.Length == 0)
            {
                Console.WriteLine($"{FriendlyName} message\n");
                return;
            }
            string sendMsg = args[0];

            //サーバーのIPアドレス（または、ホスト名）とポート番号
            string ipOrHost = "127.0.0.1";
            //string ipOrHost = "localhost";
            int port = 2001;
            int maxMessage = 10;

            //TcpClientを作成し、サーバーと接続する
            System.Net.Sockets.TcpClient tcp =
                new System.Net.Sockets.TcpClient(ipOrHost, port);
            if(tcp.Client.RemoteEndPoint is not null && tcp.Client.LocalEndPoint is not null)
            {
                Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address,
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port,
                    ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address,
                    ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port);
            }

            //NetworkStreamを取得する
            System.Net.Sockets.NetworkStream ns = tcp.GetStream();

            //読み取り、書き込みのタイムアウトを10秒にする
            //デフォルトはInfiniteで、タイムアウトしない
            //(.NET Framework 2.0以上が必要)
            ns.ReadTimeout = 10000;
            ns.WriteTimeout = 10000;

            //サーバーにデータを送信する
            //文字列をByte型配列に変換
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');

            for( int i = 0; i < maxMessage; i++)
            {
                Console.Write($"Sending message {i+1}/{maxMessage}: ");
                //データを送信する
                ns.Write(sendBytes, 0, sendBytes.Length);
                Console.WriteLine(sendMsg);

                //サーバーから送られたデータを受信する
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                byte[] resBytes = new byte[256];
                int resSize = 0;
                do
                {
                    //データの一部を受信する
                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                    //Readが0を返した時はサーバーが切断したと判断
                    if (resSize == 0)
                    {
                        Console.WriteLine("サーバーが切断しました。");
                        break;
                    }
                    //受信したデータを蓄積する
                    ms.Write(resBytes, 0, resSize);
                    //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                    // 受信を続ける
                } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                //受信したデータを文字列に変換
                string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                ms.Close();

                //末尾の\nを削除
                resMsg = resMsg.TrimEnd('\n');
                Console.WriteLine(resMsg);

                Console.WriteLine("waiting for sending");
                Thread.Sleep(1000);
            }

            //閉じる
            ns.Close();
            tcp.Close();
            Console.WriteLine("切断しました。");
        }
    }
}