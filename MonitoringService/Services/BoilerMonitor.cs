using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataModels;
using MonitoringService.Communications;
using MonitoringService.Communications.Commands;

namespace MonitoringService.Services
{
    // Handles serial communication to the Greyko burner controller
    public class BoilerMonitor : BackgroundService
    {
        private readonly ILogger<BoilerMonitor> logger;
        private readonly dnHomeDBContext dbContext;

        private readonly SerialPortCommandProcessor commandProcessor;

        private readonly GeneralInformationCommand generalInformationCommand;
        private readonly ResetFeederCounterCommand resetFeederCounterCommand;

        public BoilerMonitor(ILogger<BoilerMonitor> logger)
        {
            this.logger = logger;
            dbContext = new dnHomeDBContext();
            // TODO(iliyan): Make configurable
            commandProcessor = new SerialPortCommandProcessor(GetBoilerConfig().SerialPort);

            generalInformationCommand = new GeneralInformationCommand();
            resetFeederCounterCommand = new ResetFeederCounterCommand();


            BoilerStatus = dbContext.LatestBoiler.AsEnumerable().FirstOrDefault();
            if (BoilerStatus == null)
            {
                BoilerStatus = new LatestBoilerSample();
                dbContext.Add(BoilerStatus);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //logger.LogInformation("BoilerMonitor running at: {time}", DateTimeOffset.Now);
                MonitorBoiler();
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void MonitorBoiler()
        {
            var cfg = GetBoilerConfig();
            
            if (cfg.IsBoilerEnabled && cfg.StopBoiler)
            {
                var response = commandProcessor.ProcessCommand(new StopBoilerCommand());
                if (response != null && response is SuccessResponse)
                {
                    logger.LogInformation("Stopping boiler by configuration request");
                    // Set IsBoilerEnabled = false
                    Configuration.GetBoilerConfig().IsBoilerEnabled = false;
                    Configuration.SaveSettings();
                }
                else
                {
                    logger.LogError("Boiler not stopped!!!");
                }
            }
            else if (!cfg.IsBoilerEnabled && !cfg.StopBoiler)
            {
                var response = commandProcessor.ProcessCommand(new StartBoilerCommand());
                if (response != null && response is SuccessResponse)
                {
                    Configuration.GetBoilerConfig().IsBoilerEnabled = true;
                    Configuration.SaveSettings();
                    logger.LogInformation("Starting boiler...");
                }
                else
                {
                    logger.LogError("Boiler not started!!!");
                }
            }

            var result = commandProcessor.ProcessCommand(generalInformationCommand);
            if (result != null && result is BoilerSampleResponse)
            {
                var response = (BoilerSampleResponse)result;
                try
                {
                    if (response.FFWorkTime > 0)
                    {
                        var resetFFResult = commandProcessor.ProcessCommand(resetFeederCounterCommand);
                        if (resetFFResult != null && resetFFResult is SuccessResponse)
                        {
                            logger.LogInformation("Reset Successful");
                        }
                        else
                        {
                            logger.LogError("Reset Unsuccessful");
                        }
                    }

                    dbContext.Boiler.Add(response);
                    UpdateBoilerSample(BoilerStatus, response);
                    dbContext.Update(BoilerStatus);
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    logger.LogError("Exception saving Boiler sample data: {0}\r\n{1}", e.Message, e.InnerException);
                }
            }
            else
            {
                logger.LogInformation("Type of response is {0}", result?.GetType());
            }
        }

        public static LatestBoilerSample BoilerStatus { get; set; }

        private static void UpdateBoilerSample(LatestBoilerSample latest, BoilerSampleResponse response)
        {
            latest.Timestamp = response.Timestamp;
            latest.CurrentTemperature = response.CurrentTemperature;
            latest.BF = response.BF;
            latest.CHPump = response.CHPump;
            latest.DHW = response.DHW;
            latest.Errors = response.Errors;
            latest.FF = response.FF;
            latest.FFWorkTime = response.FFWorkTime;
            latest.Fan = response.Fan;
            latest.Flame = response.Flame;
            latest.GreykoTimestamp = response.GreykoTimestamp;
            latest.Mode = response.Mode;
            latest.State = response.State;
            latest.Status = response.Status;
            latest.Power = response.Power;

            BoilerStatus = latest;
        }

        private BoilerConfig GetBoilerConfig()
        {
            return Configuration.GetBoilerConfig();
        }
    }
}