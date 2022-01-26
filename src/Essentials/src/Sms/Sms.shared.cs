using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Sms']/Docs" />
	public static partial class Sms
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="//Member[@MemberName='ComposeAsync'][0]/Docs" />
		public static Task ComposeAsync()
			=> ComposeAsync(null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="//Member[@MemberName='ComposeAsync'][1]/Docs" />
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

	/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SmsMessage']/Docs" />
	public class SmsMessage
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public SmsMessage()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public SmsMessage(string body, string recipient)
		{
			Body = body;
			if (!string.IsNullOrWhiteSpace(recipient))
				Recipients.Add(recipient);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public SmsMessage(string body, IEnumerable<string> recipients)
		{
			Body = body;
			if (recipients != null)
			{
				Recipients.AddRange(recipients.Where(x => !string.IsNullOrWhiteSpace(x)));
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='Body']/Docs" />
		public string Body { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='Recipients']/Docs" />
		public List<string> Recipients { get; set; } = new List<string>();
	}
}
