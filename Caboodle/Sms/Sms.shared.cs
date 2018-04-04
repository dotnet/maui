using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Sms
    {
        public static Task ComposeAsync()
            => ComposeAsync(null);

        public static Task ComposeAsync(SmsMessage message)
        {
            if (!IsComposeSupported)
                throw new FeatureNotSupportedException();

            return PlatformComposeAsync(message);
        }
    }

    public class SmsMessage
    {
        public SmsMessage()
        {
        }

        public SmsMessage(string body, string recipient)
        {
            Body = body;
            Recipient = recipient;
        }

        public string Body { get; set; }

        public string Recipient { get; set; }
    }
}
