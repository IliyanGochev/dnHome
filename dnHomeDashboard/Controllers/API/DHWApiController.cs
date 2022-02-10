using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataModels;

namespace dnHomeDashboard.Controllers.API
{

    [ApiController]
    [Route("/api/v1/[controller]")]
    public class DHWController : ControllerBase
    {
        private readonly dnHomeDBContext context;
        public DHWController(dnHomeDBContext ctx)
        {
            context = ctx;
        }

        [HttpGet]
        [Route("latest")]
        // Return the latest 24 hour data aggregated in 15 minutes
        public IEnumerable<DHWSample> GetLatest()
        {
            return context.Boiler.Where(x => x.Timestamp > DateTime.UtcNow.AddHours(-24))
                            .AsEnumerable()
                            .GroupBy(x =>
                                {
                                    var stamp = new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day, x.Timestamp.Hour, x.Timestamp.Minute, 0);
                                    stamp = stamp.AddMinutes(-(stamp.Minute % 15));

                                    stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
                                    return stamp;
                                })
                            .Select(g => new DHWSample { Timestamp = g.Key, Temperature = MathF.Round(g.Average(s => (float)s.DHW), 1) });
        }

        [HttpGet]
        [Route("currentTemp")]
        public float CurrentTemp()
        {
            return context.LatestBoiler.AsEnumerable().FirstOrDefault().DHW;
        }
    }
}
