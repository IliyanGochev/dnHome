﻿using System;

namespace MonitoringService.Communications.Commands
{
    public class StopBoilerCommand : CommandBase, ICommand
    {
        protected override byte commandId { get; set; } = 0x03;
        protected override byte[] requestData { get; set; } = new byte[] {0x01, 0x02};
        protected override byte[] responseData { get; set; }

        public override byte[] GetRequestData()
        {
            return new byte[] {0x5A, 0x5A, 0x04, 0x03, 0x01, 0x02, 0xFB};
        }

        public override IResponse ProcessResponseData(byte[] response)
        {
            try
            {
                Console.Write("Stop Boiler command: " + response.Length + ", "); 
                PrintBA(response);
                if (response.Length == 4 && 
                    response[0] == 0x5a &&
                    response[1] == 0x5a &&
                    response[2] == 0x02 &&
                    response[3] == 0x34)
                {
                    response = new byte[] { 0x5a, 0x5a, 0x02, 0x34, 0xCA};
                    PrintBA(response);
                }
                else
                {
                    Console.WriteLine("Not Equal?!?");
                }
                base.ProcessResponseData(response);
                this.IsSuccessful = (responseData?.Length == 1 &&
                                     responseData[0] == 0x34);
            }
            catch (Exception e)
            {
                this.IsSuccessful = false;
                Console.WriteLine(e.Message);
            }

            if (this.IsSuccessful)
            {
                return new SuccessResponse();
            }
            else
            {
                return new FailResponse();
            }
        }

    }
}