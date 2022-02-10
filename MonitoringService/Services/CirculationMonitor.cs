using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Device.Gpio;
using System.Collections.Generic;
using DataModels;

namespace MonitoringService
{
    public class CirculationMonitor : BackgroundService
    {
        private readonly ILogger<CirculationMonitor> _logger;
        private GpioController gpioController;
        private DateTimeOffset _lastRunTime;
        public static bool IsRunning = false;

        private readonly int PinID = Configuration.GetCirculationConfig().CirculationPumpPinID;


        private List<CirculationPeriod> circulationPeriods = new List<CirculationPeriod> {
            new CirculationPeriod() {
                Start = new TimeSpan(22,30,0),
                End = new TimeSpan(6, 0,0),
                Period = new TimeSpan(8,30,0)
            },
            new CirculationPeriod() {
                Start = new TimeSpan(6,5,0),
                End = new TimeSpan(22,15,0),
                Period = new TimeSpan(0,15,0)
            }
        };

        public CirculationMonitor(ILogger<CirculationMonitor> logger)
        {
            _logger = logger;
            gpioController = new GpioController();
            gpioController.OpenPin(PinID);
            gpioController.SetPinMode(PinID, PinMode.Output);
            gpioController.Write(PinID, PinValue.High); // Relay Off
            _lastRunTime = DateTimeOffset.Now.AddMinutes(-30);
        }

        private TimeSpan circulationDuration = new TimeSpan(0, 2, 0);
        private TimeSpan offPeriodDuration;

        private bool IsPeriodInTimeSpan(CirculationPeriod period)
        {
            TimeSpan now = DateTimeOffset.Now.TimeOfDay;
            TimeSpan start = period.Start;
            TimeSpan end = period.End;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }

        private TimeSpan GetPeriodOffDuration()
        {
            foreach (var period in circulationPeriods)
            {
                if (IsPeriodInTimeSpan(period))
                {
                    return period.Period;
                }
            }
            return new TimeSpan(1, 0, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Configuration.GetCirculationConfig().IsCirculationEnabled)
                {
                    // Select Circualtion Period configuration based on the current time
                    offPeriodDuration = GetPeriodOffDuration();

                    //_logger.LogInformation("CirculationMonitor running at: {time}", DateTimeOffset.Now);
                    if ((DateTimeOffset.Now - _lastRunTime > offPeriodDuration) && IsRunning == false)
                    {
                        _lastRunTime = DateTimeOffset.Now;
                        gpioController.Write(PinID, PinValue.Low);
                        _logger.LogInformation("Starting circulation at: {time}", DateTimeOffset.UtcNow);
                        IsRunning = true;
                    }
                    else if (IsRunning)
                    {
                        if (DateTimeOffset.Now - _lastRunTime > circulationDuration)
                        {
                            // Stop circulation
                            gpioController.Write(PinID, PinValue.High);
                            IsRunning = false;
                            _logger.LogInformation("Stopping circulation at: {time}", DateTimeOffset.UtcNow);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Still {time} left until next cycle", offPeriodDuration - (DateTimeOffset.Now - _lastRunTime));
                    }
                }
                // TODO(Iliyan): Pull up the interval
                var updateInterval = 60000; // 60 Seconds
                await Task.Delay(updateInterval, stoppingToken);
            }
        }
    }
}