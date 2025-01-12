using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using MonitoringService.Communications.Commands;


namespace MonitoringService.Communications
{
    class SerialPortCommandProcessor
    {
        private SerialPort serialPort;
        private bool portOpen = false;

        public SerialPortCommandProcessor(string portName)
        {
            serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;
            serialPort.Encoding = Encoding.Unicode;
            serialPort.Handshake = Handshake.None;
            try{
                serialPort.Open();
                portOpen = true;
            }catch(Exception e) {
                Console.WriteLine("Serial Port Opening failure: " + e.Message);
            }            
        }

        ~SerialPortCommandProcessor()
        {
            if(portOpen){
                serialPort.Close();
            }
        }

        public IResponse ProcessCommand(ICommand command)
        {
            if(portOpen)
            {
                Thread.Sleep(300);
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                Thread.Sleep(300);

                byte[] requestData = command.GetRequestData();
                //Console.Write("Request: " + CommandBase.ByteArrayToHexString(requestData) + "\t");
                serialPort.Write(requestData, 0, requestData.Length);

                Thread.Sleep(500);

                string responseData = serialPort.ReadExisting();

                var responseBytes = Encoding.Unicode.GetBytes(responseData);
                IResponse response = command.ProcessResponseData(responseBytes);
                //Console.WriteLine("Response: " + CommandBase.ByteArrayToHexString(responseBytes));
                return response;
            }
            return null;
        }
    }
}
