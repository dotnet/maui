using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class SmsImplementation : ISms
	{
		public bool IsComposeSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformComposeAsync(SmsMessage message)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
