using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialCOM
{
    class Program
    {
        static void Main(string[] args)
        {
	    var serialPort = new SerialPort("/dev/ttyUSB0", 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;
            serialPort.Encoding = Encoding.Unicode;
            serialPort.NewLine = "\r";


            serialPort.Open();
            Thread.Sleep(300);
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            var requestData = new byte[] {0x5a, 0x5a, 0x02, 0x01, 0xFD};

            Console.WriteLine("Requesting " + requestData);
            //serialPort.Write(requestData, 0, requestData.Length);
            serialPort.WriteLine(requestData.ToString());
            Thread.Sleep(200);
            Console.WriteLine("Waiting for response...");
            Thread.Sleep(500);
            Console.WriteLine(serialPort.ReadExisting());

        }
    }
}
