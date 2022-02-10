using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Sms']/Docs" />
	public class SmsImplementation:ISms
	{
		internal static bool IsComposeSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task ComposeAsync(SmsMessage message)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
