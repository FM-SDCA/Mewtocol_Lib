using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// MewToColコマンド用ライブラリ
namespace MewtocolLib
{
    // FP7 Object class
    // extends TCP Client 
    public class FP7 : Http_Server_Client.Client
    {
        private struct ReadDT
        {
            public int dtNum;
            public int value;
        }

        ReadDT[] rdt;
        public int errorMode { get; private set; } = 0;
        public const int err_dtNumTooSmall = 0x01;
        public const int err_communicationFail = 0x02;


        // initialize DT vals, start setting of Comm to FP7 as a TCP Client.
        public FP7(int[] dtNums, String serverIPAddStr, int port) : base(serverIPAddStr, port) 
        {
            if (dtNums.Length > 0)
            {

                rdt = new ReadDT[dtNums.Length];
                foreach (int i in dtNums)
                {
                    rdt[i].dtNum = dtNums[i];
                    rdt[i].value = 0;       // as default value
                }
            }
            else errorMode |= err_dtNumTooSmall;
        }

        // FP7　numで指定したDTの値を読み取る。
        public int GetDT(int num)
        {
            int retry = 3;
            int ValuePosition = 2;
            int ValueCount = 5;
            string receivedMessage;

            while (true)
            {
                receivedMessage = TCPClientSend(Convert.ReadData(rdt[num].dtNum));
                int receivedValue = int.Parse(receivedMessage.Substring(ValuePosition, ValueCount));

                if (receivedMessage is null) retry--;
                else return rdt[num].value = receivedValue;

                if (retry <= 0)
                {
                    errorMode |= err_communicationFail;
                    return 0;
                }
            }

        }
        

        // FP7　numで指定したDTの値をsetValueで上書きする。
        public bool SetDT(int num, int setValue)
        {
            int ValuePosition = 2;
            int ValueCount = 5;
            int retry = 3;

            while(true)
            {
                string receivedMessage = TCPClientSend(Convert.WriteData(setValue, rdt[num].dtNum));
                int receivedValue = int.Parse(receivedMessage.Substring(ValuePosition, ValueCount));

                if (setValue != receivedValue) retry--;
                else return true;
                if (retry <= 0)
                {
                    errorMode |= err_communicationFail;
                    return false;
                }
            }
        }
        
    }

    // MewToCol変換クラス
    public static class Convert
    {
        // MewToCol Symbols
        const string header = "%";
        const string receiver = "FF"; // 全体へ送信
        const string sendSymbol = "#";
        const string writeCode = "WD";
        const string readCode = "RD";
        const string dtDataCode = "D";
        const string dummyBCC = "**";
        const char crCode = (char)0x22;

        // 送信文字列の作成：Write　numで指定したDTにvalueで上書き。
        static public string WriteData(int value, int num)
        {
            return header + receiver + sendSymbol + writeCode + dtDataCode +
                num.ToString("00000") + num.ToString("00000") + value.ToString("x4") +
                dummyBCC + crCode;
        }

        // 送信文字列の作成：Read　numで指定したDTの値を読み取り。
        static public string ReadData(int num)
        {
            return header + receiver + sendSymbol + readCode + dtDataCode +
                num.ToString("00000") + num.ToString("00000") +
                dummyBCC + crCode;
        }
    }
}
