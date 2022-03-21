using System;
using System.IO;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DataModels;
using Microsoft.EntityFrameworkCore.Design;
using GpioController = System.Device.Gpio.GpioController;

//using System.Device.Gpio;

namespace MonitoringService.Services
{
    public class DHWMonitor : BackgroundService
    {
        private DHWHeatingConfig GetHeatingConfig()
        {
            return Configuration.GetDHWConfig();
        }

        private readonly ILogger<DHWMonitor> logger;
        private dnHomeDBContext dbContext;

        private readonly String sensorID = Configuration.GetDHWConfig().DHWTemperatureProbeID;
        private readonly int ElectricityHeatingPin = Configuration.GetDHWConfig().ElectricHeatingPinID;
        private readonly int BoilerHeatingPin = Configuration.GetDHWConfig().DHWPumpPinID;

        private GpioController gpioController;

        private bool waterOverheated = false;
        private bool isElectricallyHeating = false;
        private bool isBoilerHeating = false;
        public DHWMonitor(ILogger<DHWMonitor> logger)
        {
            this.logger = logger;
            dbContext = new dnHomeDBContext();

            // Setup Raspberry Pi relay channel
            gpioController = new GpioController();

            gpioController.OpenPin(ElectricityHeatingPin);
            gpioController.SetPinMode(ElectricityHeatingPin, PinMode.Output);
            gpioController.Write(ElectricityHeatingPin, PinValue.High);

            gpioController.OpenPin(BoilerHeatingPin);
            gpioController.SetPinMode(BoilerHeatingPin, PinMode.Output);
            gpioController.Write(BoilerHeatingPin, PinValue.High);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //logger.LogInformation("DHWMonitor running at: {time}", DateTimeOffset.Now);
                MonitorDHW();
                UpdateConfig();
                CheckTemperatureRules();

                // TODO(Iliyan): Pull up the interval
                var updateInterval = 30000; // 30 Seconds
                await Task.Delay(updateInterval, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
                "DHW Monitor is stopping.");
            StopBoilerHeating();
            StopElectricHeating();
            await Task.CompletedTask;
        }

        private void UpdateConfig()
        {
            //throw new NotImplementedException();
        }

        bool IsHeatingPeriod(HeatingPeriod period)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan start = period.StartHeatingTime;
            TimeSpan end = period.StopHeatingTime;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }

        LatestBoilerSample currentBoilerSample;

        private void CheckTemperatureRules()
        {
            var config = GetHeatingConfig();
            //var waterTemp = ReadTemperatureSensor(sensorID);
            if (currentBoilerSample != null){
                dbContext.Entry(currentBoilerSample).Reload();
            }else {
                currentBoilerSample = dbContext.LatestBoiler.AsEnumerable().First();
            }

            short waterTemp = -1;

            if (DateTime.UtcNow - currentBoilerSample.Timestamp < new TimeSpan(0, 5, 0))
            {
                waterTemp = currentBoilerSample.DHW;
            }

            // Check if we have stopped the heating process manually
            if (!config.IsDHWHeatingEnabled)
            {
                logger.LogInformation("Heating is not on...");
                StopElectricHeating();
                StopBoilerHeating();
                return;
            }

            if (config.UseBoiler) // Pellet boiler
            {
                var ct = currentBoilerSample.CurrentTemperature;
                Console.WriteLine("Boiler: " + ct );
                Console.WriteLine("Boiler heating: " + isBoilerHeating);
                Console.WriteLine("Electric heating: " + isElectricallyHeating);
                var delta = ct - waterTemp;
                if (delta > 5.0f )
                {                    
                    Console.WriteLine("Boiler - DHW delta: " + delta);                    
                   if (waterTemp >= config.Max)
                    {
                        // Stop heating
                        Console.WriteLine("DHW at or above 52");
                        waterOverheated = true; 
                        StopBoilerHeating();
                    }
                    else if ((config.Min < waterTemp || waterTemp < config.Max) && waterOverheated && isBoilerHeating)
                    {                        
                        Console.WriteLine("Cooling the boiler...");
                        StopBoilerHeating();
                    }
                    else if (waterTemp <= config.Min)
                    {                        
                        Console.WriteLine("Restarting heating...");
                        StartBoilerHeating();
                        waterOverheated = false;
                    }                                  
                }
                else
                {
                    StopBoilerHeating();
                }
            }
            else
            {
                Console.WriteLine("Not using boiler for heating DHW");
                StopBoilerHeating();
            }

            if (config.UseElectricity)
            {
                // Start Heating during explicit heating periods
                /*
                if (config.HeatingPeriods != null && config.HeatingPeriods.Count > 0)
                {
                    foreach (var period in config.HeatingPeriods)
                    {
                        if (IsHeatingPeriod(period))
                        {
                            logger.LogInformation("Heating Period: " + period);
                            if (waterTemp >= config.Max)
                            {
                                StopElectricHeating();
                            }
                            else if (waterTemp < config.Max - period.Hysteresis)
                            {
                                StartElectricHeating();
                            }

                            return;
                        }
                    }

                }
                */
                if (waterTemp < config.Min) {
                    StartElectricHeating();
                } else if (waterTemp >= config.Max) {
                    StopElectricHeating();
                }
            } else { 
                StopElectricHeating(); 
            }

            if (config.ForceReheat)
            {
                StartElectricHeating();
                StartBoilerHeating();
                
            }
        }

        private void StartBoilerHeating()
        {
            if (isBoilerHeating == true)
                return;

            gpioController.Write(BoilerHeatingPin, PinValue.Low);
            isBoilerHeating = true;
            logger.LogInformation("Starting boiler heating");
        }
        private void StopBoilerHeating()
        {
            if (isBoilerHeating == false)
                return;
            gpioController.Write(BoilerHeatingPin, PinValue.High);
            isBoilerHeating = false;
            logger.LogInformation("Stopping boiler heating");
        }

        private void StartElectricHeating()
        {
            if (isElectricallyHeating == true)
                return;

            gpioController.Write(ElectricityHeatingPin, PinValue.Low);
            isElectricallyHeating = true;
            UpdateHeatingData();
            logger.LogInformation("Start heating...");
        }

        private void StopElectricHeating()
        {
            if (isElectricallyHeating == false)
                return;

            gpioController.Write(ElectricityHeatingPin, PinValue.High);
            isElectricallyHeating = false;
            UpdateHeatingData();
            logger.LogInformation("Stop heating...");
        }

        private void UpdateHeatingData()
        {
            dbContext.DHWHeating.Add(new DHWHeatingSample() { IsHeating = isElectricallyHeating, Timestamp = DateTime.UtcNow });
            dbContext.SaveChangesAsync();
        }
        private void MonitorDHW()
        {

            var temperature =  -200; // ReadTemperatureSensor(sensorID);

            if (temperature > -200)
            {
                try
                {
                    dbContext.DHW.Add(new DHWSample { Temperature = temperature, Timestamp = DateTime.UtcNow });
                    var latest = dbContext.LatestDHW.AsEnumerable().FirstOrDefault();
                    if (latest == null)
                    {
                        latest = new LatestDHWSample() { Temperature = temperature, Timestamp = DateTime.UtcNow };
                        dbContext.LatestDHW.Add(latest);
                    }
                    else
                    {
                        latest.Timestamp = DateTime.UtcNow;
                        latest.Temperature = temperature;
                        dbContext.LatestDHW.Update(latest);
                    }
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    logger.LogInformation("Exception is: {0}", e.Message);
                }
            }

            logger.LogInformation("The temp is {temperature}", temperature);
        }

        private string[] ReadTemperatureSensorRaw(string sensorID)
        {

            try
            {
                return System.IO.File.ReadAllLines("/sys/bus/w1/devices/" + sensorID + "/w1_slave");
            }
            catch (IOException ioe)
            {
                logger.LogWarning("Sensor file could not be read: {0}", ioe.Message);
            }
            return null;
        }

        private float ReadTemperatureSensor(string sensorID)
        {
            float temperature = -200;
            for (int i = 3; i > 0; i--)
            {
                string[] sensorData = ReadTemperatureSensorRaw(sensorID);
                if (sensorData != null && sensorData[0].Contains("YES"))
                {
                    var tempPart = "t=";
                    var tempStr = sensorData.Last().Substring(sensorData.Last().LastIndexOf(tempPart) + 2);
                    if (int.TryParse(tempStr, out var tempInt))
                    {
                        temperature = MathF.Round(tempInt / 1000.0f, 2);
                        break;
                    }
                    else
                    {
                        logger.LogWarning("Failed parsing the string to int...");
                    }
                }
                else
                    Thread.Sleep(2000);
            }
            return temperature;
        }
    }
}
