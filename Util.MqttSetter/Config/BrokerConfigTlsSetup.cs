namespace Util.MqttSetter.Config
{
    public class BrokerConfigTlsSetup
    {
        public bool Use { get; set; } = true;
        public bool AllowUntrusted { get; set; } = true;
        public bool IgnoreChain { get; set; } = true;
    }
}