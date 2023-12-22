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
        public byte Min { get; set; } = 45;
        public byte Max { get; set; } = 55;
        public List<HeatingPeriod> HeatingPeriods { get; set; }
        public bool IsDHWHeatingEnabled { get; set; } = true;
        public bool UseBoiler { get; set; } = false;
        public bool UseElectricity { get; set; } = true;
        public bool ForceReheat { get; set; } = false;
        public int DHWPumpPinID { get; set; } = 26;
        public int ElectricHeatingPinID { get; set; } = 21;
        public string DHWTemperatureProbeID { get; set; } = "28-0207917763b8";
    }

    public class GMailConfig
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
    }
    public class BoilerConfig
    {
        public bool IsBoilerEnabled { get; set; } = false;
        public bool StopBoiler { get; set; } = false;

        public BoilerMode Mode { get; set; } = BoilerMode.Auto;
        public BoilerPriority Priority { get; set; } = BoilerPriority.ParallelPumps;
        public string SerialPort { get; set; } = "/dev/ttyUSB0";
    }

    public class CirculationConfig
    {
        public bool IsCirculationEnabled { get; set; } = true;
        public List<CirculationPeriod> CirculationPeriods { get; set; }
        public int CirculationPumpPinID { get; set; } = 20;
    }

    public class Config
    {
        public BoilerConfig BoilerConfig { get; set; } = new BoilerConfig();
        public DHWHeatingConfig DhwHeatingConfig { get; set; } = new DHWHeatingConfig();
        public CirculationConfig CirculationConfig { get; set; } = new CirculationConfig();
        public GMailConfig GMailConfig { get; set; } = new GMailConfig();
    }
}
