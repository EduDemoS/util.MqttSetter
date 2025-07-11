namespace Util.MqttSetter.Config
{
    internal class BrokerConfig
    {
        public string Host { get; set; } = string.Empty;
        public BrokerConfigTlsSetup Tls { get; set; } = new();
        public int Port { get; set; } = 8883;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
