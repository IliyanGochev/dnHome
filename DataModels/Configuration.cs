using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DataModels
{
    public static class Configuration
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private static Config config;
        private static readonly string configFilePath = "/home/pi/dnHome/config.json";
        private static void Init()
        {
            // TODO: Move out?
            
            if (File.Exists(configFilePath))
            {
                var jsonString = File.ReadAllText(configFilePath);
                config = JsonSerializer.Deserialize<Config>(jsonString, options);
            }
            else
            {
                config = new Config();
            }
        }
        public static DHWHeatingConfig GetDHWConfig()
        {
            Init();
            var result = new DHWHeatingConfig()
            {
                Min = 45,
                Max = 65,
                // TODO: Why not properly deserialized?
                HeatingPeriods = new List<HeatingPeriod> {
                    new HeatingPeriod()
                    {
                        StartHeatingTime = new TimeSpan(3, 0, 0),
                        StopHeatingTime = new TimeSpan(7, 30, 0),
                        Hysteresis = 10
                    },
                    new HeatingPeriod()
                    {
                        StartHeatingTime = new TimeSpan(7,30,0),
                        StopHeatingTime = new TimeSpan(23,30,0),
                        Hysteresis = 4
                    }
                },
                IsDHWHeatingEnabled = true,
                UseBoiler = false,
                UseElectricity = true,
                ForceReheat = false,
                
            };

            
            if (config?.DhwHeatingConfig != null)
                result = config.DhwHeatingConfig;
            else
            {
                config.DhwHeatingConfig = result;
            }

            return result;
        }

        public static BoilerConfig GetBoilerConfig()
        {
            Init();
            var result = new BoilerConfig() {IsBoilerEnabled = true, StopBoiler = false};
           
            if (config?.BoilerConfig != null)
                result = config.BoilerConfig;
            else
            {
                config.BoilerConfig = result;
            }
            return result;
        }

        public static CirculationConfig GetCirculationConfig()
        {
            Init();
            var result = new CirculationConfig()
            {
                IsCirculationEnabled = true, 
                CirculationPeriods = new List<CirculationPeriod>
                {
                    new CirculationPeriod()
                    {
                        Start = new TimeSpan(23, 30, 0),
                        End = new TimeSpan(7, 0, 0),
                        Period = new TimeSpan(2, 30, 0)
                    },
                    new CirculationPeriod()
                    {
                        Start = new TimeSpan(6, 5, 0),
                        End = new TimeSpan(22, 15, 0),
                        Period = new TimeSpan(0, 15, 0)
                    }
                }
            };
            
            if (config?.CirculationConfig != null)
                result = config.CirculationConfig;
            else
            {
                config.CirculationConfig = result;
            }

            return result;
        }

        public static void SaveSettings()
        {
            if(config?.BoilerConfig != null && config.DhwHeatingConfig != null && config.CirculationConfig != null) {
                var jsonString = JsonSerializer.Serialize<Config>(config, options);
                File.WriteAllText(configFilePath, jsonString);
                Console.WriteLine("Writing new config file...");
            }
        }
        public static void SaveNewConfig()
        {
            if (!File.Exists(configFilePath))
            {
                if(config?.BoilerConfig != null && config.DhwHeatingConfig != null && config.CirculationConfig != null) {
                    var jsonString = JsonSerializer.Serialize<Config>(config, options);
                    File.WriteAllText(configFilePath, jsonString);
                    Console.WriteLine("Writing new config file...");
                }
                else
                {
                    if (config == null)
                        Console.WriteLine("Config is null");
                    else if (config.BoilerConfig == null)
                    {
                        Console.WriteLine("Boiler config is null");
                    }else if (config.DhwHeatingConfig == null)
                    {
                        Console.WriteLine("DHW Config is NULL!");
                    }else if (config.CirculationConfig == null)
                    {
                        Console.WriteLine("Circulation Config is NULL!");
                    }
                }
            }
        }
    }
}
