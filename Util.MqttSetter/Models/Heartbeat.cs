using System.Text.Json.Serialization;

namespace Util.MqttSetter.Models
{
    internal class Heartbeat
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = "off";

        [JsonPropertyName("mirror")]
        public bool Mirror { get; set; } = false;
    }
}
