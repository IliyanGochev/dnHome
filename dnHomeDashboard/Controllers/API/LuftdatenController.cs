using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DataModels;

namespace dnHomeDashboard.Controllers.API
{

    [ApiController]
    [Route("/api/v1/")]
    public class LuftdatenController : ControllerBase
    {
        private readonly dnHomeDBContext context;

        public LuftdatenController(dnHomeDBContext ctx)
        {
            context = ctx;
        }

        [HttpGet]
        [Route("luftdaten/get")]
        public LatestLuftdatenSample Get()
        {
            // await dbContext
            return context.LatestLuftdaten.FirstOrDefault();
        }

        [HttpGet]
        public IEnumerable<LuftdatenSample> GetFrom(DateTime timestamp)
        {
            return null;
        }

        [HttpGet]
        public IEnumerable<LuftdatenSample> GetLast24Hours()
        {
            return null;
        }

        private static string lastReq = "Init...";
        private static int count = 0;

        [HttpPost]
        [Route("luftdaten-endpoint")]
        public async Task<ActionResult<string>> Endpoint()
        {
            LuftdatenSample sample = new LuftdatenSample();

            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();

                // Do something
                lastReq = body;
                var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var obj = root.GetProperty("sensordatavalues");
                foreach (var o in obj.EnumerateArray())
                {
                    switch (o.GetProperty("value_type").GetString())
                    {
                        case "SDS_P1":
                            {
                                sample.PM_10 = float.Parse( o.GetProperty("value").GetString());
                                break;
                            }
                        case "SDS_P2":
                            {
                                sample.PM_2_5 = float.Parse( o.GetProperty("value").GetString());
                                break;
                            }
                        case "BME280_temperature":
                            {
                                sample.Temperature = Decimal.Parse(o.GetProperty("value").GetString());
                                break;
                            }
                        case "BME280_humidity":
                            {
                                sample.Humidity = Decimal.Parse(o.GetProperty("value").GetString());
                                break;
                            }
                        case "BME280_pressure":
                            {
                                sample.Pressure = Decimal.Parse(o.GetProperty("value").GetString());
                                break;
                            }
                    }
                    sample.Timestamp = DateTime.Now;
                }
            }

            context.LuftdatenStation.Add(sample);
            var latest = context.LatestLuftdaten.FirstOrDefault();
            if (latest == null)
            {
                latest = new LatestLuftdatenSample();
                latest.Timestamp = sample.Timestamp;
                latest.Humidity = sample.Humidity;
                latest.Temperature = sample.Temperature;
                latest.Pressure = sample.Pressure;
                latest.PM_10 = sample.PM_10;
                latest.PM_2_5 = sample.PM_2_5;
                context.LatestLuftdaten.Add(latest);
            }
            else
            {
                latest.Timestamp = sample.Timestamp;
                latest.Humidity = sample.Humidity;
                latest.Temperature = sample.Temperature;
                latest.Pressure = sample.Pressure;
                latest.PM_10 = sample.PM_10;
                latest.PM_2_5 = sample.PM_2_5;
                context.LatestLuftdaten.Update(latest);
            }

            context.SaveChanges();
            return new ActionResult<string>(lastReq);
        }

        [HttpGet]
        [Route("luftdaten-last")]
        public string Last()
        {
            return count++ + ": " + lastReq;
        }
    }
}