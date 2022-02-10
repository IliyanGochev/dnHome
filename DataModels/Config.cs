using System;
using System.Collections.Generic;

namespace DataModels
{
    public class HeatingPeriod
    {
        public TimeSpan StartHeatingTime { get; set; }
        public TimeSpan StopHeatingTime { get; set; }
        public float Hysteresis { get; set; } = 1.0f;

        public override string ToString()
        {
            return "Start: " + StartHeatingTime.ToString() + ", End: " + StopHeatingTime.ToString();
        }
    }

    public class CirculationPeriod
    {
        public TimeSpan Start;
        public TimeSpan End;
        public TimeSpan Period;
    }
    public class DHWHeatingConfig
    {
        public byte Min { get; set; }
        public byte Max { get; set; }
        public List<HeatingPeriod> HeatingPeriods { get; set; }
        public bool IsDHWHeatingEnabled { get; set; }
        public bool UseBoiler { get; set; }
        public bool UseElectricity { get; set; }
        public bool ForceReheat { get; set; }
        public int DHWPumpPinID { get; set; } = 21;
        public int ElectricHeatingPinID { get; set; } = 26;
        public string DHWTemperatureProbeID { get; set; } = "28-0207917763b8";
    }
    public class BoilerConfig
    {
        public bool IsBoilerEnabled { get; set; }
        public bool StopBoiler { get; set; }
        public string SerialPort { get; set; } = "/dev/ttyUSB0";
    }

    public class CirculationConfig
    {
        public bool IsCirculationEnabled { get; set; }
        public List<CirculationPeriod> CirculationPeriods { get; set; }
        public int CirculationPumpPinID { get; set; } = 20;
    }

    public class Config
    {
        public BoilerConfig BoilerConfig { get; set; }
        public DHWHeatingConfig DhwHeatingConfig { get; set; }
        public CirculationConfig CirculationConfig { get; set; }
    }
}