#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	public interface ISms
	{
		bool IsComposeSupported { get; }

		Task ComposeAsync(SmsMessage? message);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Sms']/Docs" />
	public static class Sms
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="//Member[@MemberName='ComposeAsync'][1]/Docs" />
		public static Task ComposeAsync()
			=> Current.ComposeAsync(null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="//Member[@MemberName='ComposeAsync'][2]/Docs" />
		public static Task ComposeAsync(SmsMessage? message)
			=> Current.ComposeAsync(message);

		static ISms Current => ApplicationModel.Communication.Sms.Default;

		static ISms? defaultImplementation;

		public static ISms Default =>
			defaultImplementation ??= new SmsImplementation();

		internal static void SetDefault(ISms? implementation) =>
			defaultImplementation = implementation;
	}

	partial class SmsImplementation : ISms
	{
		public Task ComposeAsync() =>
			ComposeAsync(null);

		public Task ComposeAsync(SmsMessage? message)
		{
			if (!IsComposeSupported)
				throw new FeatureNotSupportedException();

			message ??= new SmsMessage();

			message.Recipients ??= new List<string>();

			return PlatformComposeAsync(message);
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SmsMessage']/Docs" />
	public class SmsMessage
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public SmsMessage()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public SmsMessage(string body, string? recipient)
		{
			Body = body;
			if (!string.IsNullOrWhiteSpace(recipient))
				Recipients.Add(recipient!);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public SmsMessage(string body, IEnumerable<string>? recipients)
		{
			Body = body;
			if (recipients != null)
			{
				Recipients.AddRange(recipients.Where(x => !string.IsNullOrWhiteSpace(x)));
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='Body']/Docs" />
		public string? Body { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/SmsMessage.xml" path="//Member[@MemberName='Recipients']/Docs" />
		public List<string> Recipients { get; set; } = new List<string>();
	}
}
