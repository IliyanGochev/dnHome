using System;
using DataModels;

namespace MonitoringService.Communications.Commands
{
   public class GeneralInformationCommand : CommandBase, ICommand
    {
        protected const byte expectedResponseDataLength = 28;

        protected override byte commandId { get; set; } = 0x01;
        protected override byte[] requestData { get; set; }
        protected override byte[] responseData { get; set; }
	public string Error{get;set;}
        public override byte[] GetRequestData()
        {
            requestData = new byte[] { };

            return base.GetRequestData();
        }

        public override IResponse ProcessResponseData(byte[] response)
        {
            try
            {
                base.ProcessResponseData(response);

                this.IsSuccessful = (responseData?.Length == expectedResponseDataLength);
            }
            catch (Exception e)
            {
                this.IsSuccessful = false;
		        Console.WriteLine(e.Message);
            }

            if (this.IsSuccessful)
            {
                return new BoilerSampleResponse()
                {
                    // Software Version = responseData[1].ToString("X").Insert(1, "."),
                    Timestamp = DateTime.Now.ToUniversalTime(),
                    GreykoTimestamp = new DateTime(2000 + int.Parse(responseData[7].ToString("X")), int.Parse(responseData[6].ToString("X")), int.Parse(responseData[5].ToString("X")),
                                        int.Parse(responseData[2].ToString("X")), int.Parse(responseData[3].ToString("X")), int.Parse(responseData[4].ToString("X"))),
                    Mode = (BoilerMode)responseData[8],
                    State = (OperationMode)responseData[9],
                    Status = (BurnerStatus)responseData[10],
                    Errors = ((responseData[13] & (1 << 0)) != 0? Errors.IgnitionFail : 0) | ((responseData[13] & (1 << 5)) != 0 ? Errors.PelletJam : 0),
                    SetTemperature = responseData[16],
                    CurrentTemperature = responseData[17],
                    DHW =  responseData[18],
                    Flame = responseData[20],
                    Heather = (responseData[21] & (1 << 1)) != 0,
                    CHPump = (responseData[21] & (1 << 3)) != 0,
                    BF = (responseData[21] & (1 << 4)) != 0,
                    FF = (responseData[21] & (1 << 5)) != 0,
                    Fan = responseData[23],
                    Power = (BurningPower)responseData[24],
                    ThermostatStop = (responseData[25] & (1 << 7)) != 0,
                    FFWorkTime =  responseData[27]
                };
            }
           
            return new FailResponse();
        }
    }
}
