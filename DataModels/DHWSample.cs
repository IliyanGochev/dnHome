using System;
using System.ComponentModel.DataAnnotations;

namespace DataModels
{
    public class LatestDHWSample
    {
        [Key] 
        public int Id { get; set; }
        public DateTime Timestamp {get; set;}
        public float Temperature {get; set;}
    }

    public class DHWSample {
        [Key]
        public DateTime Timestamp {get; set;}
        public float Temperature {get; set;}
    }    
}