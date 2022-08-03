using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Sms']/Docs" />
	partial class SmsImplementation : ISms
	{
		public bool IsComposeSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformComposeAsync(SmsMessage message)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
