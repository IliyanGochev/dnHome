using System;
using System.Collections.Generic;
using System.Linq;
using DataModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace dnHomeDashboard.Controllers.API
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class BoilerController : ControllerBase
    {
        private readonly dnHomeDBContext context;

        public BoilerController(dnHomeDBContext ctx)
        {
            context = ctx;
        }

        [HttpGet]
        [Route("latest")]
        public IEnumerable<BoilerSample> GetLatest()
        {
            return context.Boiler.Where(x => x.Timestamp > DateTime.UtcNow.AddHours(-24))
                .AsEnumerable()
                .GroupBy(x =>
                {
                    var stamp = new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day, x.Timestamp.Hour,
                        x.Timestamp.Minute, 0);
                    stamp = stamp.AddMinutes(-(stamp.Minute % 15));

                    stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
                    return stamp;
                })
                .Select(g => new BoilerSample
                {
                    Timestamp = g.Key,
                    CurrentTemperature = (short)Math.Round(g.Average(s => s.CurrentTemperature)),
                    Flame = (short)Math.Round(g.Average(s => s.Flame)),
                    SetTemperature = (short)Math.Round(g.Average(s => s.SetTemperature)),
                    Power = (BurningPower)Math.Round(g.Average(s => (int)s.Power))
                });
        }       

        [HttpGet]
        [Route("currentTemp")]
        public LatestBoilerSample CurrentTemp()
        {
            return context.LatestBoiler.AsEnumerable().First();
        }

        [HttpGet]
        [Route("latest-trend")]
        public List<BoilerSample> latestBoilerTrend()
        {
            return context.Boiler.Where(x => x.Timestamp > DateTime.UtcNow.AddMinutes(-10)).ToList();
        }

        // TODO(iliyan): pull out magic / config numbers
        static readonly double kgPerHour = 24.5;
        static readonly double feedTime = kgPerHour / 3600;

        [HttpGet]
        [Route("consumption/24hours")]
        public double ConsumtionFor24Hours()
        {

            double result = context.Boiler.Where(x => x.Timestamp > DateTime.UtcNow.AddHours(-24) && x.FFWorkTime > 0).Select(g => g.FFWorkTime).Sum(s => s);
            return Math.Round(result * feedTime, 2);
        }

        [HttpGet]
        [Route("consumption/bymonths")]
        public IEnumerable<KeyValuePair<KeyValuePair<int, int>, double>> ConsumptionByMonths()
        {
            return context.Boiler.Where(x => x.Timestamp > DateTime.UtcNow.AddMonths(-12) && x.FFWorkTime > 0)
                .GroupBy(g => new { g.Timestamp.Year, g.Timestamp.Month })
                .Select(g => new KeyValuePair<KeyValuePair<int, int>, double>(
                    new KeyValuePair<int, int>(g.Key.Year, g.Key.Month),
                    Math.Round(g.Sum(s => s.FFWorkTime * feedTime), 2)));
        }

        public class WeeklyResult
        {
            public string Day { get; set; }
            public double Consumption { get; set; }
        }

        [HttpGet]
        [Route("consumption/by-week")]
        public IEnumerable<WeeklyResult> GetLastWeek()
        {
            return context.Boiler.Where(w => w.Timestamp.Date > DateTime.Today.AddDays(-6) && w.FFWorkTime > 0)
                .AsEnumerable()
                .GroupBy(x => x.Timestamp.Date)
                .OrderBy(g => g.Key)
                .Select(g => new WeeklyResult()
                {
                    Day = g.Key.ToShortDateString(),
                    Consumption = Math.Round(g.Sum(x => (x.FFWorkTime * feedTime)),2)
                });
        }

        [HttpGet]
        [Route("control/stop")]
        public string StopBoiler()
        {
            try
            {
                Configuration.GetBoilerConfig().StopBoiler = true;
                Configuration.GetBoilerConfig().IsBoilerEnabled = true;
                Configuration.SaveSettings();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error occured";
            }

            return "Initiated";
        }

        [HttpGet]
        [Route("control/start")]
        public string StartBoiler()
        {
            try
            {
                Configuration.GetBoilerConfig().StopBoiler = false;
                Configuration.GetBoilerConfig().IsBoilerEnabled = false;
                Configuration.SaveSettings();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Error Starting";
            }

            return "Started";
        }
    }
}
