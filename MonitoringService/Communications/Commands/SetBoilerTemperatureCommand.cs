namespace MonitoringService.Communications.Commands
{
    public class SetBoilerTemperatureCommand : CommandBase, ICommand
    {
        protected override byte commandId { get; set; } = 0x07;
        protected override byte[] requestData { get; set; }
        protected override byte[] responseData { get; set; }
        protected byte _boilerTemperature;

        public SetBoilerTemperatureCommand(byte boilerTemperature)
        {
            _boilerTemperature = boilerTemperature;
        }

        public override byte[] GetRequestData()
        {
            requestData = new byte[] { _boilerTemperature };

            return base.GetRequestData();
        }

        public override IResponse ProcessResponseData(byte[] response)
        {
            try
            {
                base.ProcessResponseData(response);

                this.IsSuccessful = (responseData?.Length == 1 &&
                                     responseData[0] == 0x34);
            }
            catch
            {
                this.IsSuccessful = false;
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