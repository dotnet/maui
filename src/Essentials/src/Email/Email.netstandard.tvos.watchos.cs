using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformComposeAsync(EmailMessage message) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
