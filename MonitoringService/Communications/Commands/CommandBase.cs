using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MonitoringService.Communications.Commands
{
    public abstract class CommandBase
    {
        protected byte[] header = { 0x5A, 0x5A };
        protected abstract byte commandId { get; set; }
        protected abstract byte[] requestData { get; set; }
        protected abstract byte[] responseData { get; set; }

        public bool IsSuccessful { get; protected set; }

        public virtual byte[] GetRequestData()
        {
            List<byte> request = new List<byte>
            {
                // data length = 1 byte command Id + requestData.Length + 1 byte checksum
                (byte)(requestData.Length + 2),
                // command Id
                commandId
            };

            // request data
            for (byte n = 0; n < requestData.Length; n++)
            {
                request.Add((byte)(requestData[n]));
            }

            // checksum
            request.Add((byte)(CalculateCheckSum(request.ToArray()) + request.Count - 1));

            // increment request data values
            for (byte n = 2; n < requestData.Length + 2; n++)
            {
                request[n] = (byte)(request[n] + n - 1);
            }

            // header
            request.InsertRange(0, header);

            return request.ToArray();
        }

        public static void PrintBA(byte[] ba)
        {
            Console.WriteLine(ByteArrayToHexString(ba));
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public virtual IResponse ProcessResponseData(byte[] response)
        {
            if (response.Length < header.Length + 2)
            {
                throw new Exception("Invalid response");
            }

            // check header
            for (int n = 0; n < header.Length; n++)
            {
                if (response[n] != header[n])
                {
                    throw new Exception("Invalid response header");
                }
            }

            // check length
            var expected = (header.Length + 1 + response[header.Length]);
            if (response.Length !=  expected)
            {
                PrintBA(response);
                throw new Exception("Invalid response length: " + response.Length + ", but expected: " + expected);
            }

            List<byte> data = new List<byte>();
            for (byte n = 2; n < response.Length - 1; n++)
            {
                data.Add(response[n]);
            }

            // decrement response data values
            for (byte n = 1; n < data.Count; n++)
            {
                data[n] = (byte)(data[n] - n + 1);
            }

            var origCheckSum = data[^1];

            int sum = data.ToArray().Sum(d => d);
            var b = BitConverter.GetBytes(sum);
            byte checkSum =(byte)(((byte) sum & 0xFF) ^ 0xFF);

            // checksum validation
            if (response[^1] != ((byte)(checkSum + (data.Count - 1))&0xFF))
            {
                PrintBA(response);
                throw new Exception("Response checksum validation failed" );
            }

            data.RemoveAt(0);

            responseData = data.ToArray();

            return null; // do not return result in the base class
        }
        private byte CalculateCheckSum(byte[] data)
        {
            int sum = data.Sum(d => d);

            return (byte)(((byte)sum & 0xFF) ^ 0xFF);
        }
    }
}
