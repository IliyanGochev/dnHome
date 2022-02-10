using System;
using System.ComponentModel.DataAnnotations;

namespace DataModels
{
    // Will be used to store / display the data from a Luftdaten metering station through Web API
    public class LuftdatenSample
    {
        [Key]
        public DateTime Timestamp {get; set;}
        public decimal  Temperature {get; set;}
        public decimal  Humidity {get; set;}
        public decimal  Pressure {get; set;}
        public float      PM_2_5 {get; set;}
        public float      PM_10  {get;set;}
    }

    public class LatestLuftdatenSample
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp {get; set;}
        public decimal  Temperature {get; set;}
        public decimal  Humidity {get; set;}
        public decimal  Pressure {get; set;}
        public float      PM_2_5 {get; set;}
        public float      PM_10  {get;set;}
    }
}
