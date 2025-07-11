using MQTTnet;
using Util.MqttSetter.Config;

namespace Util.MqttSetter
{
    public static class MqttClientOptionsBuilderExtensions
    {
        public static MqttClientOptionsBuilder WithTls(this MqttClientOptionsBuilder builder, BrokerConfigTlsSetup tlsSetup)
        {
            if (tlsSetup.Use)
            {
                var tlsOptionBuilder = new MqttClientTlsOptionsBuilder()
                                       .UseTls()

                                       .WithIgnoreCertificateRevocationErrors()
                                       .WithCertificateValidationHandler(context => true);

                if (tlsSetup.AllowUntrusted)
                {
                    tlsOptionBuilder = tlsOptionBuilder.WithAllowUntrustedCertificates();
                }

                if (tlsSetup.IgnoreChain)
                {
                    tlsOptionBuilder = tlsOptionBuilder.WithIgnoreCertificateChainErrors();
                }


                var tlsOptions = tlsOptionBuilder.Build();

                builder = builder.WithTlsOptions(tlsOptions);
            }

            return builder;
        }
    }
}
