using System;

namespace MonitoringService.Communications.Commands
{
    public class ResetFeederCounterCommand : CommandBase, ICommand
    {
        protected override byte commandId { get; set; } = 0x09;
        protected override byte[] requestData { get; set; }
        protected override byte[] responseData { get; set; }

        public ResetFeederCounterCommand()
        {
        }

        public override byte[] GetRequestData()
        {
            requestData = new byte[] { };

            return base.GetRequestData();
        }

        public override IResponse ProcessResponseData(byte[] response)
        {
            try
            {
                Console.Write("Feed Reset length: " + response.Length + ", "); 
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