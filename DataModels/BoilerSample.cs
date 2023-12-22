using System;
using System.ComponentModel.DataAnnotations;

namespace DataModels
{
    public enum BoilerMode
    {
        None = -1,
        Standby = 0,
        Auto,
        Timer
    }

    public enum BurningPower
    {
        Idle = 0,
        Suspend,
        Power1,
        Power2,
        Power3
    }

    public enum BoilerPriority
    {
        None = -1,
        CHPriority = 0,
        DHWPriority,
        ParallelPumps,
        SummerMode
    }
    public enum BurnerStatus
    {
        Idle = 0,
        FanCleaning,
        Cleaner,
        Wait,
        Loading,
        Heating,
        Ignition1,
        Ignition2,
        Unfolding,
        Burning,
        Extinction
    }

    [Flags]
    public enum Errors
    {
        NoError = 0,
        IgnitionFail,
        PelletJam
    }
    public class BoilerSample
    {
        [Key]
        public DateTime Timestamp { get; set; }
        public DateTime GreykoTimestamp { get; set; }
        public BoilerMode Mode { get; set; }
        public BurnerStatus Status { get; set; }
        public BoilerPriority State { get; set; } // TODO(iliyan): check the old DB for this?
        public Errors Errors { get; set; }
        public short SetTemperature { get; set; } // Do I need it in the DB?
        public short CurrentTemperature { get; set; }
        public short Flame { get; set; }
        public bool Heather { get; set; } // TODO(iliyan): check the old DB for this?
        public short DHW { get; set; }
        public bool CHPump { get; set; }
        public bool BF { get; set; }
        public bool FF { get; set; }
        public short Fan { get; set; }
        public BurningPower Power { get; set; }
        public bool ThermostatStop { get; set; } //TODO(iliyan): Do I need it in the DB?
        public short FFWorkTime { get; set; }
    }

    public class LatestBoilerSample
    {
        [Key] 
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime GreykoTimestamp { get; set; }
        public BoilerMode Mode { get; set; }
        public BurnerStatus Status { get; set; }
        public BoilerPriority State { get; set; } // TODO(iliyan): check the old DB for this?
        public Errors Errors { get; set; }
        public short SetTemperature { get; set; } // Do I need it in the DB?
        public short CurrentTemperature { get; set; }
        public short Flame { get; set; }
        public bool Heather { get; set; } // TODO(iliyan): check the old DB for this?
        public short DHW { get; set; }
        public bool CHPump { get; set; }
        public bool BF { get; set; }
        public bool FF { get; set; }
        public short Fan { get; set; }
        public BurningPower Power { get; set; }
        public bool ThermostatStop { get; set; } //TODO(iliyan): Do I need it in the DB?
        public short FFWorkTime { get; set; }
    }
}