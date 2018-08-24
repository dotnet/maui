using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Sms
    {
        public static Task ComposeAsync()
            => ComposeAsync(null);

        public static Task ComposeAsync(SmsMessage message)
        {
            if (!IsComposeSupported)
                throw new FeatureNotSupportedException();

            if (message == null)
                message = new SmsMessage();

            if (message?.Recipients == null)
                message.Recipients = new List<string>();

            return PlatformComposeAsync(message);
        }
    }

    public class SmsMessage
    {
        public SmsMessage()
        {
        }

        public SmsMessage(string body, IEnumerable<string> recipients)
        {
            Body = body;
            Recipients = recipients?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        public string Body { get; set; }

        public List<string> Recipients { get; set; }
    }
}
