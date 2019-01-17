using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http_Server_Client
{
    public class Client
    {
        System.Net.Sockets.TcpClient tcp;
        System.Net.Sockets.NetworkStream ns;
        

        public Client(String serverIPAddStr, int port)
        {
            TCPClient(serverIPAddStr, port);
        }

        public String TCPClient(String serverIPAddStr, int port)
        {

            // サーバーと接続する

            tcp = new System.Net.Sockets.TcpClient(serverIPAddStr, port);

            // NetworkStreamを取得する
            ns = tcp.GetStream();

            return ("サーバー(" +
                ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address + " : " +
                ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port + ")と接続しました(" +
                ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address + " : " +
                ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port) + ")。\r\n";
        }

        public String TCPClientSend(String sendMsg)
        {
            ns.ReadTimeout = 10000;
            ns.WriteTimeout = 10000;

            // サーバーにデータを送信

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
            do
            {
                // データの一部を受信する
                resSize = ns.Read(resBytes, 0, resBytes.Length);
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

            return  ClientListening();
            
        }

        public String ClientListening()
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


        public String DisconnectFromServer()
        {

            // 閉じる
            ns.Close();
            tcp.Close();

            return "TCPサーバーとの接続を切断しました。";
        }
    }
}
