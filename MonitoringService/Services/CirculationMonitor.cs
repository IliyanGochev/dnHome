using DataModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringService
{
    public class CirculationMonitor : BackgroundService
    {
        private readonly ILogger<CirculationMonitor> _logger;
        private GpioController _gpioController;
        private DateTimeOffset _lastRunTime;
        public static bool IsRunning = false;

        private readonly int _pinId = Configuration.GetCirculationConfig().CirculationPumpPinID;
        // TODO(Iliyan): Create configuration option, recheck it periodically?
        private readonly TimeSpan _circulationDuration = new TimeSpan(0, 2, 0);

        private static readonly TimeSpan CirculationOnPeriod = new TimeSpan(0, 15, 0);
        private static readonly TimeSpan CirculationOffPeriod = new TimeSpan(12, 0, 0);

        // Weekday circulation periods
        private readonly List<CirculationPeriod> _weeklyCirculationPeriods = new List<CirculationPeriod> {
            new CirculationPeriod() {
                Start = new TimeSpan(22,30,0),
                End = new TimeSpan(6, 30,0),
                Period = CirculationOffPeriod
            },
            new CirculationPeriod() {
                Start = new TimeSpan(6,35,0),
                End = new TimeSpan(10,0,0),
                Period = CirculationOnPeriod
            },
            new CirculationPeriod() {
                Start = new TimeSpan(12,0,0),
                End = new TimeSpan(14,0,0),
                Period = CirculationOnPeriod
            },
            new CirculationPeriod() {
                Start = new TimeSpan(16, 30, 0),
                End = new TimeSpan(22, 25, 0),
                Period = CirculationOnPeriod
            }
        };

        private readonly List<CirculationPeriod> _weekendCirculationPeriods = new List<CirculationPeriod> {
             new CirculationPeriod() {
                Start = new TimeSpan(22,30,0),
                End = new TimeSpan(6, 50,0),
                Period = CirculationOffPeriod
            },
            new CirculationPeriod() {
                Start = new TimeSpan(6,55,0),
                End = new TimeSpan(22,10,0),
                Period = CirculationOnPeriod
            },
        };

        public CirculationMonitor(ILogger<CirculationMonitor> logger)
        {
            _logger = logger;
            _gpioController = new GpioController();
            _gpioController.OpenPin(_pinId);
            _gpioController.SetPinMode(_pinId, PinMode.Output);
            _gpioController.Write(_pinId, PinValue.High); // Relay Off
            _lastRunTime = DateTimeOffset.Now.AddMinutes(-30);
        }

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

        private TimeSpan GetPeriodOffDuration(List<CirculationPeriod> periods)
        {
            foreach (var period in periods)
            {
                if (IsPeriodInTimeSpan(period))
                {
                    return period.Period;
                }
            }
            return new TimeSpan(4, 0, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Configuration.GetCirculationConfig().IsCirculationEnabled)
                {
                    var dow = DateTime.UtcNow.DayOfWeek;
                    if (dow != DayOfWeek.Saturday | dow != DayOfWeek.Sunday)
                    {
                        CheckRunningConditions(_weeklyCirculationPeriods);
                    }
                    else
                    {
                        CheckRunningConditions(_weekendCirculationPeriods);
                    }

                }
                // TODO(Iliyan): Pull up the interval
                var updateInterval = 60000; // 60 Seconds
                await Task.Delay(updateInterval, stoppingToken);
            }
        }

        private void CheckRunningConditions(List<CirculationPeriod> periods)
        {
            // Select Circulation Period configuration based on the current time
            var offPeriodDuration = GetPeriodOffDuration(periods);

            //_logger.LogInformation("CirculationMonitor running at: {time}", DateTimeOffset.Now);
            if ((DateTimeOffset.Now - _lastRunTime > offPeriodDuration) && IsRunning == false)
            {
                _lastRunTime = DateTimeOffset.Now;
                _gpioController.Write(_pinId, PinValue.Low);
                _logger.LogInformation("Starting circulation at: {time}", DateTimeOffset.UtcNow);
                IsRunning = true;
            }
            else if (IsRunning)
            {
                if (DateTimeOffset.Now - _lastRunTime > _circulationDuration)
                {
                    // Stop circulation
                    _gpioController.Write(_pinId, PinValue.High);
                    IsRunning = false;
                    _logger.LogInformation("Stopping circulation at: {time}", DateTimeOffset.UtcNow);
                }
            }
            else
            {
                _logger.LogInformation("Still {time} left until next cycle",
                    offPeriodDuration - (DateTimeOffset.Now - _lastRunTime));
            }
        }
    }
}
