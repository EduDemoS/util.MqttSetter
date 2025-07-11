using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MQTTnet;
using Util.MqttSetter.Models;
using Util.MqttSetter.Config;

namespace Util.MqttSetter
{
    class Program
    {
        private static readonly IConfigurationRoot config = new ConfigurationBuilder()
                                                            .SetBasePath(Directory.GetCurrentDirectory())
                                                            .AddJsonFile("appsettings.default.json", optional: true, reloadOnChange: true)
                                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                            .Build();
        private static readonly BrokerConfig BrokerSetup = new();

        static Program()
        {
            config.Bind("Broker", BrokerSetup);
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
                await mqttClient.ConnectAsync(options);

                bool state = false;
                List<string> teacherDevices = ["SUNFLOWERies", "SUNFLOWER00", "SUNFLOWERitq"];
                var devices = teacherDevices.Union(GenerateDeviceList("SUNFLOWER", 1, 15));

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
                await SendPayloadToDevices(mqttClient, "testresult", devices, heartbeatOff);
                await SendPayloadToDevices(mqttClient, "testresult", teacherDevices, testresultDone);

                await mqttClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
            }
        }

        private static async Task SendPayloadToDevices(IMqttClient mqttClient, string topicName, IEnumerable<string> devices, string payload, int delayMs = 0)
        {
            foreach (var device in devices)
            {
                var topic = $"EduDemoS/{device}/data/{topicName}";
                var message = new MqttApplicationMessageBuilder()
                              .WithTopic(topic)
                              .WithPayload(payload)
                              .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                              .Build();

                Console.Write($"Sening {topic}: {payload}...");
                await mqttClient.PublishAsync(message);
                Console.WriteLine($"done");

                await Task.Delay(delayMs);
            }
        }

        static List<string> GenerateDeviceList(string basename, int min, int max)
        {
            var list = new List<string>();
            for (int i = min; i <= max; i++)
            {
                list.Add($"{basename}{i:D2}");
            }
            return list;
        }
    }
}
