using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace DataModels
{
    public class DHWHeatingSample
    {
        [Key]
        public DateTime Timestamp { get; set; }
        public bool IsHeating { get; set; }
    }
}
