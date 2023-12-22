using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringService.Communications.Commands
{
    internal class ChangeModeAndPriorityCommand : CommandBase, ICommand
    {

        // b1-2: 0x5A 0x5A - Header
        // b3  : 0x04 -  lenght (includes checksum)
        // b4  : 0x03 - commandID - change Priority and mode
        // b5  : Mode:
        //              0x01 - StandBy
        //              0x02 - Auto
        //              0x03 - Timer
        // b6  : Priority:
        //              0x02 - Heating
        //              0x03 - DHW Priority
        //              0x04 - Parallel Pumps
        //              0x05 - Summer Mode

        public ChangeModeAndPriorityCommand(byte Mode, byte Priority)
        {
            requestData = Array.Empty<byte>();
            requestData = requestData.Append(Mode).Append(Priority).ToArray();
            PrintBA(requestData);
        }
        protected override byte commandId { get; set; } = 0x03; // Change mode and priority
        protected override byte[] requestData { get; set; }
        protected override byte[] responseData { get; set; }

        public override IResponse ProcessResponseData(byte[] response)
        {
            try
            {
                Console.Write("Change Boiler Mode and Priority command: " + response.Length + ", ");
                PrintBA(response);
                if (response.Length == 4 &&
                    response[0] == 0x5a &&
                    response[1] == 0x5a &&
                    response[2] == 0x02 &&
                    response[3] == 0x34)
                {
                    response = new byte[] { 0x5a, 0x5a, 0x02, 0x34, 0xCA };
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
