using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MQTTnet;
using Util.MqttSetter.Models;
using Util.MqttSetter.Config;
using System.Text.RegularExpressions;
using MQTTnet.Protocol;

namespace Util.MqttSetter
{
    class Program
    {
        private static readonly IConfigurationRoot config = new ConfigurationBuilder()
                                                            .SetBasePath(Directory.GetCurrentDirectory())
                                                            .AddJsonFile("appsettings.default.json", optional: true, reloadOnChange: true)
                                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                            .AddJsonFile("setup.default.json", optional: false, reloadOnChange: true)
                                                            .AddJsonFile("setup.json", optional: true, reloadOnChange: true)
                                                            .Build();
        private static readonly BrokerConfig BrokerSetup = new();
        private static readonly SettingConfig Setup = new();

        static Program()
        {
            config.Bind("Broker", BrokerSetup);
            config.Bind("Setup", Setup);
        }

        static async Task Main(string[] args)
        {
            var factory = new MqttClientFactory();
            using var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                          .WithTcpServer(BrokerSetup.Host, BrokerSetup.Port)
                          .WithCredentials(BrokerSetup.User, BrokerSetup.Password)
                          .WithTls(BrokerSetup.Tls)
                          .WithCleanSession()
                          .Build();

            try
            {
                bool state = false;

                await mqttClient.ConnectAsync(options);

                var teacherDevices = BuildDeviceNames(Setup.DevicePrefixes, Setup.TeacherIds);
                var teamDevices = BuildDeviceNames(Setup.DevicePrefixes, Setup.TeamIds);
                var devices = teacherDevices.Union(teamDevices);

                while (!Console.KeyAvailable)
                {
                    var heartbeat = JsonSerializer.Serialize(new Heartbeat()
                    {
                        State = state ? "on" : "off",
                        Mirror = false
                    });

                    var testresults = JsonSerializer.Serialize(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                    await SendPayloadToDevices(mqttClient, "heartbeat", devices, heartbeat, 100);
                    await SendPayloadToDevices(mqttClient, "testresult", devices, testresults, 100);

                    state = !state;
                }

                var heartbeatOff = JsonSerializer.Serialize(new Heartbeat()
                {
                    Mirror = false,
                    State = "off"
                });

                var testresultDone = JsonSerializer.Serialize(new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 });
                await SendPayloadToDevices(mqttClient, "heartbeat", devices, heartbeatOff);
                await SendPayloadToDevices(mqttClient, "testresult", teacherDevices, testresultDone);

                await mqttClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
            }
        }

        private static IEnumerable<string> BuildDeviceNames(IEnumerable<string> prefixes, IEnumerable<string> ids)
        {
            foreach (var basename in prefixes)
            {
                foreach (var id in ids)
                {
                    if (int.TryParse(id, out var val))
                    {
                        yield return $"{basename}{val:D2}";
                    }
                    else
                    {
                        yield return $"{basename}{id}";
                    }
                }
            }
        }

        private static async Task SendPayloadToDevices(IMqttClient mqttClient, string topicName, IEnumerable<string> devices, string payload, int delayMs = 0)
        {
            foreach (var device in devices)
            {
                string topic = BuildFullyQualifiedTopicName(Setup.WorkshopId, device, topicName);

                var message = new MqttApplicationMessageBuilder()
                              .WithTopic(topic)
                              .WithPayload(payload)
                              .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                              .Build();

                Console.Write($"Sening {topic}: {payload}...");
                await mqttClient.PublishAsync(message);
                Console.WriteLine($"done");

                await Task.Delay(delayMs);
            }
        }

        private static string BuildFullyQualifiedTopicName(string workshopId, string device, string topicName)
        {
            var topic = $"EduDemoS/";
            
            // Sanitize all topic fragments
            workshopId = SanitizeMqttTopicFragment(workshopId);
            device = SanitizeMqttTopicFragment(device);
            topicName = SanitizeMqttTopicFragment(topicName);
            
            if (!string.IsNullOrEmpty(Setup.WorkshopId))
            {
                topic += $"{workshopId}/";
            }
            topic += $"{device}/data/{topicName}";

            return topic;
        }

        private static string SanitizeMqttTopicFragment(string fragment)
        {
            return Regex.Replace(fragment, @"[^a-zA-Z0-9_-]", "_");
        }
    }
}
