using System;
using System.Collections.Generic;
using System.Text;

namespace MonitoringService.Communications
{
    public interface ICommand
    {
        byte[] GetRequestData();
        IResponse ProcessResponseData(byte[] response);
        bool IsSuccessful { get; }
    }
}
