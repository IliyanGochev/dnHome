using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataModels;
using MailKit.Net.Smtp;
using MimeKit;
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

        private BoilerMode currentMode;
        private BoilerPriority currentPriority;

        public BoilerMonitor(ILogger<BoilerMonitor> logger)
        {
            this.logger = logger;
            dbContext = new dnHomeDBContext();
            commandProcessor = new SerialPortCommandProcessor(GetBoilerConfig().SerialPort);

            generalInformationCommand = new GeneralInformationCommand();
            resetFeederCounterCommand = new ResetFeederCounterCommand();


            BoilerStatus = dbContext.LatestBoiler.AsEnumerable().FirstOrDefault();
            if (BoilerStatus == null)
            {
                BoilerStatus = new LatestBoilerSample();
                dbContext.Add(BoilerStatus);
            }

            var cfg = GetBoilerConfig();
            currentMode = cfg.Mode;
            currentPriority = cfg.Priority;

            Console.WriteLine($"Boiler mode {currentMode}, priority: {currentPriority}");
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

        private byte getModeByte(BoilerMode mode)
        {
            switch (mode)
            {

                case BoilerMode.Standby:
                    return 0x00;
                case BoilerMode.Auto:
                    return 0x01;
                case BoilerMode.Timer:
                    return 0x02;
                default:
                    return 0x00;
            }
        }

        private byte getPriorityByte(BoilerPriority priority)
        {
            switch (priority)
            {
                case BoilerPriority.DHWPriority:
                    return 0x01;
                case BoilerPriority.ParallelPumps:
                    return 0x02;
                case BoilerPriority.SummerMode:
                    return 0x03;
                case BoilerPriority.CHPriority:
                default:
                    return 0x00;
            }
        }

        private BoilerMode previousMode { get; set; } = BoilerMode.None;
        private BoilerPriority previousPriority { get; set; } = BoilerPriority.None;

        private Errors previousErrorState { get; set; } = Errors.NoError;
        private bool errorHandled { get; set; } = false;

        private void MonitorBoiler()
        {
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
                            //logger.LogInformation("Reset Successful");
                        }
                        else
                        {
                            logger.LogError("Reset Unsuccessful");
                        }
                    }

                    var cfg = GetBoilerConfig();
                    var gmailCfg = Configuration.GetGMailConfig();


                    // There's an error and it hasn't been handled
                    if (response.Errors != Errors.NoError && !errorHandled)
                    {

                        if (response.Errors == Errors.IgnitionFail)
                        {
                            logger.LogError("Ignition error");
                            SendMail(gmailCfg, "Проблем със запалването!", "Boiler: Ignition Fail!");
                        }
                        else if (response.Errors == Errors.IgnitionFail)
                        {
                            logger.LogError("Pelet Jam Error");
                            SendMail(gmailCfg, "Задръстване с пелети!", "Boiler: Pelet Jam!");
                        }
                    }

                    if (previousErrorState != Errors.NoError && response.Errors == Errors.NoError)
                    {
                        // Error has been cleared, reset errorHandled
                        errorHandled = false;
                    }
                    previousErrorState = response.Errors;

                    // start of the program, initialize the setting
                    if (previousMode == BoilerMode.None) previousMode = response.Mode;
                    if (previousPriority == BoilerPriority.None) previousPriority = response.State;

                    currentMode = response.Mode;
                    currentPriority = response.State;

                    // Handle local change at the burner
                    if (currentMode != previousMode)
                    {
                        logger.LogInformation("Mode changed at the burner!");
                        cfg.Mode = currentMode;
                        Configuration.UpdateBoilerConfig(cfg);
                        Configuration.SaveSettings();
                        previousMode = currentMode;
                        SendMail(gmailCfg, $"Mode changed to: {currentMode}", "Boiler: Mode Changed!");
                    }

                    if (currentPriority != previousPriority)
                    {
                        logger.LogInformation("Priority changed at the burner!");
                        cfg.Priority = currentPriority;
                        Configuration.UpdateBoilerConfig(cfg);
                        Configuration.SaveSettings();
                        previousPriority = currentPriority;
                        SendMail(gmailCfg, $"Priority changed to: {currentPriority}", "Boiler: Priority Changed!");
                    }

                    // We've changed the config file
                    if (cfg.Mode != currentMode || cfg.Priority != currentPriority)
                    {
                        logger.LogInformation($"PrevMode: {previousMode}, prevPriority: {previousPriority}\r\n currMode: {currentMode}, currPriority: {currentPriority}\r\n" +
                                              $"cfgMode: {cfg.Mode}, cfgPriority: {cfg.Priority}");
                        logger.LogInformation($"Changing mode to: {cfg.Mode} from {currentMode} and priority to {cfg.Priority} from {currentPriority}");

                        try
                        {
                            var cmd = new ChangeModeAndPriorityCommand(getModeByte(cfg.Mode), getPriorityByte(cfg.Priority));
                            var cmdResponse = commandProcessor.ProcessCommand(cmd);

                            if (cmdResponse != null && cmdResponse is SuccessResponse)
                            {
                                var msg = $"Changed mode to: {cfg.Mode} and priority to {cfg.Priority}";
                                logger.LogInformation(msg);
                                SendMail(gmailCfg, msg, "Boiler Mode Changed");
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e.ToString());
                        }
                    }


                    var existingBoiler = dbContext.Boiler.Find(response.Timestamp);
                    if (existingBoiler == null)
                    {
                        dbContext.Boiler.Add(response);
                    }
                    else
                    {
                        var msg = $"Updating existing boiler sample, timestamp: {response.Timestamp}";
                        logger.LogInformation(msg);
                        dbContext.Entry(existingBoiler).CurrentValues.SetValues(response);
                        SendMail(gmailCfg, msg, "Record updated, not added");
                    }                    
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

        // TODO: Extract to utility function
        private void SendMail(GMailConfig gmailCfg, string mailBody, string subject)
        {
            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress("Sender Name", gmailCfg.Sender));
                email.To.Add(new MailboxAddress("Receiver Name", gmailCfg.Receiver));

                email.Subject = subject;

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $"<b>{mailBody}</b>"
                };


                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 465, true);

                // Note: only needed if the SMTP server requires authentication
                smtp.Authenticate(gmailCfg.User, gmailCfg.Password);

                smtp.Send(email);
                smtp.Disconnect(true);
                errorHandled = true;
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
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