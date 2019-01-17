using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http_Server_Client
{
    public class Server
    {

        System.Net.Sockets.TcpListener listener;
        System.Net.Sockets.TcpClient client;
        System.Net.Sockets.NetworkStream ns;

        public Server(String ipAddStr, int port)
        {
            ServerSetup(ipAddStr, port);
        }

        public Server()
        {
            ServerSetup("127.0.0.1", 2001);
        }

        public void ServerSetup(String ipAddStr, int port)
        {
            //ListenするIPアドレス
            System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipAddStr);

            //ホスト名からIPアドレスを取得する時は、次のようにする
            //string host = "localhost";
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.GetHostEntry(host).AddressList[0];
            //.NET Framework 1.1以前では、以下のようにする
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.Resolve(host).AddressList[0];

            //TcpListenerオブジェクトを作成する
            listener = new System.Net.Sockets.TcpListener(ipAdd, port);
        }

        public String ServerListen()
        {
            //Listenを開始する
            listener.Start();
            return "Listenを開始しました(" +
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Address + " : " +
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Port + ")\r\n";
        }

        public String ServerListeningStart()
        {
            //接続要求があったら受け入れる
            client = listener.AcceptTcpClient();

            //NetworkStreamを取得
            ns = client.GetStream();

            return ("クライアント(" +
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address + " : " +
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port) + ")と接続しました。";
        }

        public String ServerListening()
        {

            if (ns.DataAvailable)
            {

                //読み取り、書き込みのタイムアウトを10秒にする
                //デフォルトはInfiniteで、タイムアウトしない
                //(.NET Framework 2.0以上が必要)
                ns.ReadTimeout = 10000;
                ns.WriteTimeout = 10000;

                //クライアントから送られたデータを受信する
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                bool disconnected = false;
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                byte[] resBytes = new byte[256];
                int resSize = 0;
                do
                {
                    //データの一部を受信する
                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                    //Readが0を返した時はクライアントが切断したと判断
                    if (resSize == 0)
                    {
                        disconnected = true;
                        Console.WriteLine("クライアントが切断しました。");
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

                if (!disconnected)
                {
                    //クライアントにデータを送信する
                    //クライアントに送信する文字列を作成
                    string sendMsg = resMsg.Length.ToString();
                    //文字列をByte型配列に変換
                    byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
                    //データを送信する
                    ns.Write(sendBytes, 0, sendBytes.Length);
                    Console.WriteLine(sendMsg);
                }
                return resMsg;
            }

            else return "";
        }

        public String StopListening()
        {
            String msg;

            //閉じる
            ns.Close();
            client.Close();
            msg = ("クライアントとの接続を閉じました。\r\n");

            //リスナを閉じる
            listener.Stop();
            msg += ("Listenerを閉じました。\r\n");

            return msg;

            //Console.ReadLine();
        }


        public String TCPServerSend(String sendMsg)
        {

            // 読み取り、書き込みのタイムアウトを10秒にする
            // デフォルトはInfiniteで、タイムアウトしない
            // (.NET Framework 2.0以上が必要)
            ns.ReadTimeout = 1500;
            ns.WriteTimeout = 1500;

            // サーバーにデータを送信する
            // 文字列をByte型配列に変換
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
            // データを送信する
            ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine(sendMsg);

            // サーバーから送られたデータを受信する
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;
            try
            {
                do
                {
                    // データの一部を受信する
                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                    // Readが0を返した時はサーバーが切断したと判断
                    if (resSize == 0)
                    {
                        Console.WriteLine("サーバーが切断しました。");
                        break;
                    }
                    // 受信したデータを蓄積する
                    ms.Write(resBytes, 0, resSize);
                    // まだ読み取れるデータがあるか、データの最後が\nでない時は、
                    // 受信を続ける
                } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                // 受信したデータを文字列に変換
                string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                ms.Close();
                // 末尾の\nを削除
                resMsg = resMsg.TrimEnd('\n');
                Console.WriteLine(resMsg);

                return sendMsg + "\r\n";
            }
            catch
            {
                return "応答時間:" + ns.ReadTimeout + "msを過ぎたため、メッセージをキャンセルしました。";
            }
        }

        public bool isConnected()
        {
            Console.WriteLine(((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address);
            Console.WriteLine(((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);

            if (((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }
    }

}