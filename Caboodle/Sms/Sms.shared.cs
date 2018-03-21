using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Sms
    {
        public static Task ComposeAsync()
            => ComposeAsync(null);
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
